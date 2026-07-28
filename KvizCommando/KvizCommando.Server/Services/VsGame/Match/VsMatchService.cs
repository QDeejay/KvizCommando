using KvizCommando.Server.Hubs;
using KvizCommando.Server.Models;
using KvizCommando.Server.Services.PlayerCache;
using KvizCommando.Server.Services.VsGame.Matchmaking;
using KvizCommando.Shared.Contracts.VsGame.Match;
using KvizCommando.Shared.Models;
using KvizCommando.Shared.Models.Enums.VsGame;
using Microsoft.AspNetCore.SignalR;
using System.Text.Json;

namespace KvizCommando.Server.Services.VsGame.Match;

public sealed class VsMatchService : IVsMatchService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly VsMatchStore _store;
    private readonly IVsMatchQuestionLoader _questionLoader;
    private readonly IHubContext<VsMatchHub, IVsMatchHubClient> _hub;
    private readonly ILogger<VsMatchService> _logger;

    public VsMatchService(
        IServiceScopeFactory scopeFactory,
        VsMatchStore store,
        IVsMatchQuestionLoader questionLoader,
        IHubContext<VsMatchHub, IVsMatchHubClient> hub,
        ILogger<VsMatchService> logger)
    {
        _scopeFactory = scopeFactory;
        _store = store;
        _questionLoader = questionLoader;
        _hub = hub;
        _logger = logger;
    }

    public async Task CreateLockedMatchAsync(
        IReadOnlyList<VsRankedQueueEntry> entries,
        CancellationToken ct = default)
    {
        if (entries.Count != VsMatchProfiles.Ranked.RequiredPlayers)
        {
            throw new InvalidOperationException(
                "A ranked match must be locked with the configured player count.");
        }

        var classificationId = entries[0].ClassificationId;

        if (entries.Any(entry =>
                entry.ClassificationId != classificationId))
        {
            throw new InvalidOperationException(
                "A ranked match cannot contain multiple classifications.");
        }

        var classification =
            VsBattleClassificationRules.List.First(rule =>
                rule.ClassificationId == classificationId);

        var match = new VsMatchSession
        {
            Profile = VsMatchProfiles.Ranked,
            Classification = classification,
            Players =
            [
                .. entries.Select((entry, index) =>
                    CreateLockedPlayer(
                        entry,
                        index + 1,
                        classification.RequiredPartySize))
            ]
        };

        if (!_store.TryAdd(match))
        {
            match.Dispose();
            throw new InvalidOperationException(
                "The locked VS match could not be registered.");
        }

        AddLog(match, null, "MatchLocked",
            $"Classification={classificationId}");

        foreach (var player in match.Players)
            await LockStakeAsync(player, classification.Stake, ct);

        await BroadcastMatchAsync(match, ct);

        var seeds = new List<VsMatchPlayerSeed>(match.Players.Count);

        foreach (var player in match.Players)
            seeds.Add(await BuildPlayerSeedAsync(player, ct));

        var loadouts = await _questionLoader.LoadAsync(
            seeds,
            match.Profile.LoadoutSize,
            ct);

        await match.Lock.WaitAsync(ct);
        try
        {
            foreach (var seed in seeds)
            {
                var player = match.Players.First(item =>
                    item.PlayerId == seed.PlayerId);

                player.DisplayName = seed.DisplayName;
                player.TeamName = seed.TeamName;
                player.TeamLevel = seed.TeamLevel;
                player.Characters = seed.Characters;
                player.HelpCounts = seed.HelpCounts;
                player.Loadout = loadouts[seed.PlayerId];
            }

            StartPhaseLocked(
                match,
                VsMatchPhase.PreparationOrder);
        }
        finally
        {
            match.Lock.Release();
        }

        await BroadcastMatchAsync(match, ct);
    }

    public Task SelectCharacterAsync(
        string connectionId,
        int slotNumber,
        CancellationToken ct = default) =>
        ExecutePreparationCommandAsync(
            connectionId,
            VsMatchPhase.PreparationOrder,
            (match, player) =>
            {
                if (player.IsFinished ||
                    player.Characters.All(character =>
                        character.SlotNumber != slotNumber) ||
                    player.Rounds.Any(round =>
                        round.CharacterSlotNumber == slotNumber))
                {
                    return false;
                }

                var target = player.Rounds.FirstOrDefault(round =>
                    !round.IsCaptainRound &&
                    !round.CharacterSlotNumber.HasValue);

                if (target is null)
                    return false;

                target.CharacterSlotNumber = slotNumber;
                AddLog(
                    match,
                    player.PlayerId,
                    "PreparationCharacterSelected",
                    $"Round={target.RoundNumber};Slot={slotNumber}");
                return true;
            },
            ct);

    public Task AssignLoadoutAsync(
        string connectionId,
        VsLoadoutAssignmentRequest request,
        CancellationToken ct = default) =>
        ExecutePreparationCommandAsync(
            connectionId,
            VsMatchPhase.PreparationCategories,
            (match, player) =>
            {
                if (player.IsFinished)
                    return false;

                var loadout = player.Loadout.FirstOrDefault(item =>
                    item.Token == request.LoadoutToken);

                var target = player.Rounds.FirstOrDefault(round =>
                    !round.IsCaptainRound &&
                    round.RoundNumber == request.RoundNumber);

                if (loadout is null ||
                    target is null ||
                    loadout.IsOwnQuestion)
                {
                    return false;
                }

                var previous = player.Rounds.FirstOrDefault(round =>
                    round.LoadoutToken == request.LoadoutToken);

                if (previous is not null)
                    previous.LoadoutToken = null;

                target.LoadoutToken = request.LoadoutToken;
                AddLog(
                    match,
                    player.PlayerId,
                    "PreparationLoadoutAssigned",
                    $"Round={target.RoundNumber};Token={request.LoadoutToken}");
                return true;
            },
            ct);

    public Task AssignHelpAsync(
        string connectionId,
        VsHelpAssignmentRequest request,
        CancellationToken ct = default) =>
        ExecutePreparationCommandAsync(
            connectionId,
            VsMatchPhase.PreparationHelps,
            (match, player) =>
            {
                if (player.IsFinished ||
                    request.HelpType is <= VsHelpType.None or >
                        VsHelpType.AiSuggestion)
                {
                    return false;
                }

                var target = player.Rounds.FirstOrDefault(round =>
                    round.RoundNumber == request.RoundNumber);

                if (target is null ||
                    !CanUseHelpInRound(
                        request.HelpType,
                        target.IsCaptainRound))
                {
                    return false;
                }

                var available = player.HelpCounts[
                    (int)request.HelpType - 1];

                var alreadyAssigned = player.Rounds.Count(round =>
                    round != target &&
                    round.HelpType == request.HelpType);

                if (alreadyAssigned >= available)
                    return false;

                target.HelpType = request.HelpType;
                AddLog(
                    match,
                    player.PlayerId,
                    "PreparationHelpAssigned",
                    $"Round={target.RoundNumber};Help={request.HelpType}");
                return true;
            },
            ct);

    public Task ResetPreparationAsync(
        string connectionId,
        CancellationToken ct = default) =>
        ExecuteAnyPreparationCommandAsync(
            connectionId,
            (match, player) =>
            {
                if (player.IsFinished)
                    return false;

                switch (match.Phase)
                {
                    case VsMatchPhase.PreparationOrder:
                        foreach (var round in player.Rounds)
                            round.CharacterSlotNumber = null;
                        break;

                    case VsMatchPhase.PreparationCategories:
                        foreach (var round in player.Rounds)
                            round.LoadoutToken = null;
                        break;

                    case VsMatchPhase.PreparationHelps:
                        foreach (var round in player.Rounds)
                            round.HelpType = VsHelpType.None;
                        break;

                    default:
                        return false;
                }

                AddLog(
                    match,
                    player.PlayerId,
                    "PreparationReset",
                    string.Empty);
                return true;
            },
            ct);

    public async Task FinishPreparationAsync(
        string connectionId,
        CancellationToken ct = default)
    {
        if (!_store.TryGetByConnection(connectionId, out var match) ||
            match is null)
        {
            return;
        }

        var changed = false;

        await match.Lock.WaitAsync(ct);
        try
        {
            var player = match.FindByConnection(connectionId);

            if (player is null ||
                player.IsFinished ||
                !CanFinish(match.Phase, player))
            {
                return;
            }

            player.IsFinished = true;
            changed = true;
            AddLog(
                match,
                player.PlayerId,
                "PreparationFinished",
                string.Empty);

            AdvanceIfReadyLocked(match);
        }
        finally
        {
            match.Lock.Release();
        }

        if (changed)
            await BroadcastMatchAsync(match, ct);
    }

    public async Task DisconnectAsync(
        string connectionId,
        CancellationToken ct = default)
    {
        if (!_store.TryGetByConnection(connectionId, out var match) ||
            match is null)
        {
            return;
        }

        await match.Lock.WaitAsync(ct);
        try
        {
            var player = match.FindByConnection(connectionId);

            if (player is null || !player.IsConnected)
                return;

            player.IsConnected = false;
            ApplyTimeoutDefaults(match, player);
            player.IsFinished = true;

            AddLog(
                match,
                player.PlayerId,
                "Disconnected",
                string.Empty);

            AdvanceIfReadyLocked(match);
        }
        finally
        {
            match.Lock.Release();
        }

        await BroadcastMatchAsync(match, CancellationToken.None);
    }

    private async Task ExecutePreparationCommandAsync(
        string connectionId,
        VsMatchPhase requiredPhase,
        Func<VsMatchSession, VsMatchPlayerState, bool> command,
        CancellationToken ct)
    {
        await ExecuteCommandAsync(
            connectionId,
            (match, player) =>
                match.Phase == requiredPhase &&
                command(match, player),
            ct);
    }

    private async Task ExecuteAnyPreparationCommandAsync(
        string connectionId,
        Func<VsMatchSession, VsMatchPlayerState, bool> command,
        CancellationToken ct)
    {
        await ExecuteCommandAsync(
            connectionId,
            (match, player) =>
                (match.Phase is
                    VsMatchPhase.PreparationOrder or
                    VsMatchPhase.PreparationCategories or
                    VsMatchPhase.PreparationHelps) &&
                command(match, player),
            ct);
    }

    private async Task ExecuteCommandAsync(
        string connectionId,
        Func<VsMatchSession, VsMatchPlayerState, bool> command,
        CancellationToken ct)
    {
        if (!_store.TryGetByConnection(connectionId, out var match) ||
            match is null)
        {
            return;
        }

        var changed = false;

        await match.Lock.WaitAsync(ct);
        try
        {
            var player = match.FindByConnection(connectionId);

            if (player is null ||
                !player.IsConnected ||
                HasExpired(match))
            {
                return;
            }

            changed = command(match, player);
        }
        finally
        {
            match.Lock.Release();
        }

        if (changed)
            await BroadcastMatchAsync(match, ct);
    }

    private async Task<VsMatchPlayerSeed> BuildPlayerSeedAsync(
        VsMatchPlayerState matchPlayer,
        CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var cache =
            scope.ServiceProvider.GetRequiredService<IPlayerCacheService>();

        VsMatchPlayerSeed? seed = null;

        var success = await cache.UpdateQuestionsLockedAsync(
            matchPlayer.PlayerId,
            matchPlayer.SessionId,
            (player, questions) =>
            {
                var selectedSlots = player.BattleTeamSlots.ToArray();

                if (selectedSlots.Length !=
                    matchPlayer.Rounds.Count(round =>
                        !round.IsCaptainRound))
                {
                    return null;
                }

                var characters = selectedSlots
                    .Select(slot => player.Characters[slot - 1])
                    .ToArray();

                if (characters.Any(character => character is null))
                    return null;

                var loadout = JsonSerializer.Deserialize<int[]>(
                                  player.Loadout.FactorySlotsJson) ??
                              [];

                var helpData = JsonSerializer.Deserialize<int[]>(
                                   player.Loadout.HelpLevelsJson) ??
                               [];

                seed = new VsMatchPlayerSeed
                {
                    PlayerId = matchPlayer.PlayerId,
                    Position = matchPlayer.Position,
                    SessionId = matchPlayer.SessionId,
                    ConnectionId = matchPlayer.ConnectionId,
                    DisplayName = player.Core.DisplayName,
                    TeamName = player.Core.TeamName,
                    TeamLevel = player.Core.RankEnum,
                    LoadoutCategories =
                    [
                        .. loadout.Take(
                            VsMatchProfiles.Ranked.LoadoutSize)
                    ],
                    HelpCounts = BuildHelpCounts(helpData),
                    Characters =
                    [
                        .. selectedSlots.Select((slot, index) =>
                            BuildCharacterState(
                                slot,
                                characters[index]!))
                    ],
                    OwnQuestions =
                    [
                        .. questions.uSlots
                            .Where(question =>
                                question is not null &&
                                question.CategoryNo > 0)
                            .Select(question => new VsOwnQuestionSeed
                            {
                                QuestionId = question.Id,
                                Question = question.Question,
                                CategoryId = question.CategoryNo,
                                AnswersJson = question.AnswersJson
                            })
                    ]
                };

                return 0u;
            },
            ct);

        if (success != true || seed is null)
        {
            throw new InvalidOperationException(
                $"Locked VS player {matchPlayer.PlayerId} could not be snapshotted.");
        }

        return seed;
    }

    private async Task LockStakeAsync(
        VsMatchPlayerState matchPlayer,
        int stake,
        CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var cache =
            scope.ServiceProvider.GetRequiredService<IPlayerCacheService>();

        var success = await cache.UpdatePlayerLockedAsync(
            matchPlayer.PlayerId,
            matchPlayer.SessionId,
            player =>
            {
                if (player.Core.Credit < stake)
                    return null;

                player.Core.Credit -= stake;
                return DirtyFlags.Core;
            },
            ct);

        if (success != true)
        {
            throw new InvalidOperationException(
                $"Locked VS player {matchPlayer.PlayerId} stake could not be reserved.");
        }

        matchPlayer.StakeLocked = true;
    }

    private static VsMatchPlayerState CreateLockedPlayer(
        VsRankedQueueEntry entry,
        int position,
        int teamSize) =>
        new()
        {
            PlayerId = entry.PlayerId,
            Position = position,
            SessionId = entry.SessionId,
            ConnectionId = entry.ConnectionId,
            DisplayName = entry.DisplayName,
            TeamName = entry.TeamName,
            TeamLevel = entry.TeamLevel,
            Rounds =
            [
                .. Enumerable.Range(1, teamSize + 1)
                    .Select(roundNumber => new VsMatchRoundState
                    {
                        RoundNumber = roundNumber,
                        IsCaptainRound = roundNumber == teamSize + 1
                    })
            ]
        };

    private static VsMatchCharacterState BuildCharacterState(
        int slotNumber,
        CharachterSlot character)
    {
        var effectiveLevel = Math.Max(character.Rank, 1);
        var modifiers = new Dictionary<int, double>();

        for (var index = 0; index < 4; index++)
        {
            AddModifier(
                modifiers,
                character.Attitude.Main.CatNo[index],
                ModifierTable.DataMainSkill[index].StartValue +
                (effectiveLevel - 1) *
                ModifierTable.DataMainSkill[index].StepValue);

            var secondaryLevel = Math.Clamp(
                character.Attitude.Secondary.Level[index],
                0,
                ModifierTable.Data.Count - 1);

            AddModifier(
                modifiers,
                character.Attitude.Secondary.CatNo[index],
                ModifierTable.Data[secondaryLevel]
                    .Modifier[index] ?? 0);

            var genderLevel = Math.Clamp(
                character.Attitude.Gender.Level[index],
                0,
                ModifierTable.Data.Count - 1);

            AddModifier(
                modifiers,
                character.Attitude.Gender.CatNo[index],
                ModifierTable.Data[genderLevel]
                    .Modifier[index + 4] ?? 0);
        }

        var orientationId = character.Attitude.Main.CatNo[0];
        if (orientationId > 8)
            orientationId -= 8;

        return new VsMatchCharacterState
        {
            SlotNumber = slotNumber,
            Name = character.Name,
            PictureCode = character.PictureCode,
            Level = character.Rank,
            OrientationId = orientationId,
            CategoryModifiers = modifiers
        };
    }

    private static void AddModifier(
        IDictionary<int, double> modifiers,
        int categoryId,
        double value)
    {
        if (categoryId is <
                VsLoadoutCategoryIds.MinimumFactoryCategory or >
                VsLoadoutCategoryIds.MaximumFactoryCategory)
        {
            return;
        }

        modifiers.TryGetValue(
           categoryId,
           out var currentValue);

        modifiers[categoryId] =
            currentValue + value;
    }

    private static int[] BuildHelpCounts(int[] helpData)
    {
        var result = new int[4];

        for (var index = 0; index < result.Length; index++)
        {
            var sourceIndex = index + 4;
            result[index] = sourceIndex < helpData.Length
                ? Math.Max(helpData[sourceIndex], 0)
                : 0;
        }

        return result;
    }

    private void StartPhaseLocked(
        VsMatchSession match,
        VsMatchPhase phase)
    {
        match.PhaseTimerCts.Cancel();
        match.PhaseTimerCts.Dispose();
        match.PhaseTimerCts = new CancellationTokenSource();
        match.Phase = phase;
        match.PhaseVersion++;

        foreach (var player in match.Players)
        {
            player.IsFinished =
                !player.IsConnected ||
                (phase == VsMatchPhase.PreparationHelps &&
                 player.HelpCounts.All(count => count <= 0));
        }

        if (phase == VsMatchPhase.PreparationCompleted)
        {
            match.DeadlineUtc = null;
            AddLog(match, null, "PreparationCompleted", string.Empty);
            return;
        }

        if (phase == VsMatchPhase.PreparationHelps &&
            match.Players.All(player => player.IsFinished))
        {
            StartPhaseLocked(
                match,
                VsMatchPhase.PreparationCompleted);
            return;
        }

        match.DeadlineUtc = DateTime.UtcNow.AddSeconds(
            match.Profile.PreparationSeconds);

        AddLog(match, null, "PhaseStarted", phase.ToString());

        _ = RunPhaseTimerAsync(
            match.MatchId,
            match.PhaseVersion,
            match.DeadlineUtc.Value,
            match.PhaseTimerCts.Token);
    }

    private async Task RunPhaseTimerAsync(
        Guid matchId,
        long phaseVersion,
        DateTime deadlineUtc,
        CancellationToken ct)
    {
        try
        {
            var delay = deadlineUtc - DateTime.UtcNow;
            if (delay > TimeSpan.Zero)
                await Task.Delay(delay, ct);

            if (!_store.TryGet(matchId, out var match) ||
                match is null)
            {
                return;
            }

            await match.Lock.WaitAsync(ct);
            try
            {
                if (match.PhaseVersion != phaseVersion ||
                    match.DeadlineUtc != deadlineUtc)
                {
                    return;
                }

                foreach (var player in match.Players
                             .Where(player => !player.IsFinished))
                {
                    ApplyTimeoutDefaults(match, player);
                    player.IsFinished = true;
                    AddLog(
                        match,
                        player.PlayerId,
                        "PreparationTimeout",
                        match.Phase.ToString());
                }

                AdvanceIfReadyLocked(match);
            }
            finally
            {
                match.Lock.Release();
            }

            await BroadcastMatchAsync(
                match,
                CancellationToken.None);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "VS phase timer failed. matchId={MatchId}",
                matchId);
        }
    }

    private void AdvanceIfReadyLocked(VsMatchSession match)
    {
        if (match.Players.Any(player => !player.IsFinished))
            return;

        switch (match.Phase)
        {
            case VsMatchPhase.PreparationOrder:
                StartPhaseLocked(
                    match,
                    VsMatchPhase.PreparationCategories);
                break;

            case VsMatchPhase.PreparationCategories:
                StartPhaseLocked(
                    match,
                    match.Players.Any(player =>
                        player.HelpCounts.Any(count => count > 0))
                        ? VsMatchPhase.PreparationHelps
                        : VsMatchPhase.PreparationCompleted);
                break;

            case VsMatchPhase.PreparationHelps:
                StartPhaseLocked(
                    match,
                    VsMatchPhase.PreparationCompleted);
                break;
        }
    }

    private static void ApplyTimeoutDefaults(
        VsMatchSession match,
        VsMatchPlayerState player)
    {
        switch (match.Phase)
        {
            case VsMatchPhase.PreparationOrder:
            {
                var usedSlots = player.Rounds
                    .Where(round =>
                        round.CharacterSlotNumber.HasValue)
                    .Select(round =>
                        round.CharacterSlotNumber!.Value)
                    .ToHashSet();

                var remainingSlots = player.Characters
                    .Where(character =>
                        !usedSlots.Contains(character.SlotNumber))
                    .Select(character => character.SlotNumber)
                    .ToQueue();

                foreach (var round in player.Rounds.Where(round =>
                             !round.IsCaptainRound &&
                             !round.CharacterSlotNumber.HasValue))
                {
                    round.CharacterSlotNumber =
                        remainingSlots.Dequeue();
                }

                break;
            }

            case VsMatchPhase.PreparationCategories:
            {
                var usedTokens = player.Rounds
                    .Where(round => round.LoadoutToken.HasValue)
                    .Select(round => round.LoadoutToken!.Value)
                    .ToHashSet();

                var remainingTokens = player.Loadout
                    .Where(item =>
                        !item.IsOwnQuestion &&
                        !usedTokens.Contains(item.Token))
                    .OrderBy(item => item.LoadoutPosition)
                    .Select(item => item.Token)
                    .ToQueue();

                foreach (var round in player.Rounds.Where(round =>
                             !round.IsCaptainRound &&
                             !round.LoadoutToken.HasValue))
                {
                    round.LoadoutToken =
                        remainingTokens.Dequeue();
                }

                break;
            }
        }
    }

    private static bool CanFinish(
        VsMatchPhase phase,
        VsMatchPlayerState player) =>
        phase switch
        {
            VsMatchPhase.PreparationOrder =>
                player.Rounds
                    .Where(round => !round.IsCaptainRound)
                    .All(round =>
                        round.CharacterSlotNumber.HasValue),

            VsMatchPhase.PreparationCategories =>
                player.Rounds
                    .Where(round => !round.IsCaptainRound)
                    .All(round => round.LoadoutToken.HasValue),

            VsMatchPhase.PreparationHelps => true,
            _ => false
        };

    private static bool CanUseHelpInRound(
        VsHelpType helpType,
        bool isCaptainRound) =>
        helpType switch
        {
            VsHelpType.FiftyFifty => true,
            VsHelpType.GuessRange => !isCaptainRound,
            VsHelpType.TimeFreeze => !isCaptainRound,
            VsHelpType.AiSuggestion => true,
            _ => false
        };

    private static bool HasExpired(VsMatchSession match) =>
        match.DeadlineUtc.HasValue &&
        DateTime.UtcNow >= match.DeadlineUtc.Value;

    private async Task BroadcastMatchAsync(
        VsMatchSession match,
        CancellationToken ct)
    {
        (string ConnectionId, VsMatchSnapshot Snapshot)[] messages;

        await match.Lock.WaitAsync(ct);
        try
        {
            messages =
            [
                .. match.Players
                    .Where(player => player.IsConnected)
                    .Select(player => (
                        player.ConnectionId,
                        BuildSnapshot(match, player)))
            ];
        }
        finally
        {
            match.Lock.Release();
        }

        foreach (var message in messages)
        {
            await _hub.Clients
                .Client(message.ConnectionId)
                .MatchChanged(message.Snapshot);
        }
    }

    private static VsMatchSnapshot BuildSnapshot(
        VsMatchSession match,
        VsMatchPlayerState currentPlayer)
    {
        var preparation = BuildPreparation(
            match,
            currentPlayer);

        return new VsMatchSnapshot
        {
            MatchId = match.MatchId,
            PhaseVersion = match.PhaseVersion,
            ClassificationId =
                match.Classification.ClassificationId,
            Stake = match.Classification.Stake,
            Phase = match.Phase,
            DeadlineUtc = match.DeadlineUtc,
            PhaseDurationSeconds =
                match.Phase is
                    VsMatchPhase.PreparationOrder or
                    VsMatchPhase.PreparationCategories or
                    VsMatchPhase.PreparationHelps
                    ? match.Profile.PreparationSeconds
                    : 0,
            InfoKey = ResolveInfoKey(match, currentPlayer),
            Players =
            [
                .. match.Players.Select(player =>
                    new VsMatchPlayerDto
                    {
                        Position = player.Position,
                        DisplayName = player.DisplayName,
                        TeamName = player.TeamName,
                        TeamLevel = player.TeamLevel,
                        TeamPictureCode =
                            player.TeamPictureCode,
                        IsMe =
                            player.PlayerId ==
                            currentPlayer.PlayerId,
                        IsConnected = player.IsConnected,
                        IsFinished = player.IsFinished
                    })
            ],
            Preparation = preparation
        };
    }

    private static VsPreparationDto BuildPreparation(
        VsMatchSession match,
        VsMatchPlayerState currentPlayer)
    {
        var assignedCharacterSlots = currentPlayer.Rounds
            .Where(round =>
                round.CharacterSlotNumber.HasValue)
            .Select(round =>
                round.CharacterSlotNumber!.Value)
            .ToHashSet();

        var assignedLoadoutTokens = currentPlayer.Rounds
            .Where(round => round.LoadoutToken.HasValue)
            .Select(round => round.LoadoutToken!.Value)
            .ToHashSet();

        return new VsPreparationDto
        {
            TeamSize = match.Classification.RequiredPartySize,
            IsFinished = currentPlayer.IsFinished,
            CanReset = CanReset(match.Phase, currentPlayer),
            CanFinish = !currentPlayer.IsFinished &&
                        CanFinish(match.Phase, currentPlayer),
            Rounds =
            [
                .. currentPlayer.Rounds.Select(round =>
                    BuildRound(currentPlayer, round))
            ],
            CharacterInventory =
            [
                .. currentPlayer.Characters
                    .Where(character =>
                        !assignedCharacterSlots.Contains(
                            character.SlotNumber))
                    .Select(ToCharacterDto)
            ],
            LoadoutInventory =
            [
                .. currentPlayer.Loadout
                    .Where(item =>
                        !assignedLoadoutTokens.Contains(item.Token))
                    .OrderBy(item => item.LoadoutPosition)
                    .Select(ToLoadoutDto)
            ],
            HelpInventory =
            [
                .. Enum.GetValues<VsHelpType>()
                    .Where(help => help != VsHelpType.None)
                    .Select(help => new VsHelpCardDto
                    {
                        HelpType = help,
                        Count = Math.Max(
                            0,
                            currentPlayer.HelpCounts[(int)help - 1] -
                            currentPlayer.Rounds.Count(round =>
                                round.HelpType == help))
                    })
            ],
            CategoryModifiers = BuildCategoryModifiers(
                match,
                currentPlayer)
        };
    }

    private static VsPreparationRoundDto BuildRound(
        VsMatchPlayerState player,
        VsMatchRoundState round)
    {
        var character = round.CharacterSlotNumber.HasValue
            ? player.Characters.FirstOrDefault(item =>
                item.SlotNumber ==
                round.CharacterSlotNumber.Value)
            : null;

        var loadout = round.LoadoutToken.HasValue
            ? player.Loadout.FirstOrDefault(item =>
                item.Token == round.LoadoutToken.Value)
            : null;

        return new VsPreparationRoundDto
        {
            RoundNumber = round.RoundNumber,
            IsCaptainRound = round.IsCaptainRound,
            Character = character is null
                ? null
                : ToCharacterDto(character),
            Loadout = loadout is null
                ? null
                : ToLoadoutDto(loadout),
            HelpType = round.HelpType
        };
    }

    private static VsCategoryModifierDto[] BuildCategoryModifiers(
        VsMatchSession match,
        VsMatchPlayerState currentPlayer)
    {
        var result = new List<VsCategoryModifierDto>();

        foreach (var round in currentPlayer.Rounds.Where(round =>
                     !round.IsCaptainRound))
        {
            for (var categoryId =
                     VsLoadoutCategoryIds.MinimumFactoryCategory;
                 categoryId <=
                     VsLoadoutCategoryIds.MaximumFactoryCategory;
                 categoryId++)
            {
                var seconds = 0d;

                foreach (var otherPlayer in match.Players.Where(player =>
                             player.PlayerId != currentPlayer.PlayerId))
                {
                    var otherRound =
                        otherPlayer.Rounds.First(item =>
                            item.RoundNumber == round.RoundNumber);

                    if (!otherRound.CharacterSlotNumber.HasValue)
                        continue;

                    var character =
                        otherPlayer.Characters.First(item =>
                            item.SlotNumber ==
                            otherRound.CharacterSlotNumber.Value);

                    seconds += character.CategoryModifiers
                        .GetValueOrDefault(categoryId);
                }

                result.Add(new VsCategoryModifierDto
                {
                    RoundNumber = round.RoundNumber,
                    CategoryId = categoryId,
                    Seconds = Math.Truncate(seconds * 10) / 10
                });
            }
        }

        return [.. result];
    }

    private static VsCharacterCardDto ToCharacterDto(
        VsMatchCharacterState character) =>
        new()
        {
            SlotNumber = character.SlotNumber,
            Name = character.Name,
            PictureCode = character.PictureCode,
            Level = character.Level,
            OrientationId = character.OrientationId
        };

    private static VsLoadoutCardDto ToLoadoutDto(
        VsMatchLoadoutItemState item) =>
        new()
        {
            LoadoutToken = item.Token,
            LoadoutPosition = item.LoadoutPosition,
            CategoryId = item.CategoryId,
            IsOwnQuestion = item.IsOwnQuestion,
            IsAllCategories = item.IsAllCategories,
            IsSelectable = !item.IsOwnQuestion
        };

    private static bool CanReset(
        VsMatchPhase phase,
        VsMatchPlayerState player) =>
        !player.IsFinished &&
        (phase switch
         {
             VsMatchPhase.PreparationOrder =>
                 player.Rounds.Any(round =>
                     round.CharacterSlotNumber.HasValue),
             VsMatchPhase.PreparationCategories =>
                 player.Rounds.Any(round =>
                     round.LoadoutToken.HasValue),
             VsMatchPhase.PreparationHelps =>
                 player.Rounds.Any(round =>
                     round.HelpType != VsHelpType.None),
             _ => false
         });

    private static string ResolveInfoKey(
        VsMatchSession match,
        VsMatchPlayerState player)
    {
        if (player.IsFinished &&
            match.Phase is
                VsMatchPhase.PreparationOrder or
                VsMatchPhase.PreparationCategories or
                VsMatchPhase.PreparationHelps)
        {
            return "vsgame.Match.Info.WaitingForPlayers";
        }

        return match.Phase switch
        {
            VsMatchPhase.MatchLocked =>
                "vsgame.Match.Info.Locked",
            VsMatchPhase.PreparationOrder =>
                "vsgame.Match.Info.Order",
            VsMatchPhase.PreparationCategories =>
                "vsgame.Match.Info.Categories",
            VsMatchPhase.PreparationHelps =>
                "vsgame.Match.Info.Helps",
            VsMatchPhase.PreparationCompleted =>
                "vsgame.Match.Info.PreparationCompleted",
            _ => "vsgame.Match.Info.Aborted"
        };
    }

    private static void AddLog(
        VsMatchSession match,
        int? playerId,
        string eventType,
        string data)
    {
        match.EventLog.Add(new VsMatchEventLogEntry
        {
            Phase = match.Phase,
            PlayerId = playerId,
            EventType = eventType,
            Data = data
        });
    }
}

internal static class VsMatchEnumerableExtensions
{
    internal static Queue<T> ToQueue<T>(
        this IEnumerable<T> source) =>
        new(source);
}

/**
 * A MatchLocked állapot után lefoglalja a téteket, snapshotolja a
 * csapatokat, egyszer betölti a meccs kérdéseit, majd szerverórával
 * végigviszi a három preparációs fázist és személyre szabott
 * SignalR-snapshotokat küld.
 */

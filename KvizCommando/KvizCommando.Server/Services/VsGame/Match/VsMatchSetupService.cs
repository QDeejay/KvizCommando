using KvizCommando.Server.Models;
using KvizCommando.Server.Services.PlayerCache;
using KvizCommando.Shared.Models;
using KvizCommando.Shared.Models.Enums.VsGame;
using System.Text.Json;

namespace KvizCommando.Server.Services.VsGame.Match;

public sealed class VsMatchSetupService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IVsMatchQuestionLoader _questionLoader;
    private readonly ILogger<VsMatchSetupService> _logger;

    public VsMatchSetupService(
        IServiceScopeFactory scopeFactory,
        IVsMatchQuestionLoader questionLoader,
        ILogger<VsMatchSetupService> logger)
    {
        _scopeFactory = scopeFactory;
        _questionLoader = questionLoader;
        _logger = logger;
    }

    internal async Task<bool> InitializePlayersAsync(
        VsMatchSession match,
        CancellationToken ct)
    {
        foreach (var player in match.Players)
        {
            await LockStakeAsync(
                player,
                match.Classification.Stake,
                ct);
        }

        lock (match.SyncRoot)
        {
            if (match.Players.All(player => !player.IsConnected))
                return false;
        }

        var seeds = new List<VsMatchPlayerSeed>(
            match.Players.Count);

        foreach (var player in match.Players)
        {
            seeds.Add(await BuildPlayerSeedAsync(
                player,
                match.Profile.LoadoutSize,
                ct));
        }

        var loadouts = await _questionLoader.LoadAsync(
            seeds,
            match.Profile.LoadoutSize,
            ct);

        lock (match.SyncRoot)
        {
            if (match.IsClosed ||
                match.Players.All(player => !player.IsConnected))
            {
                return false;
            }

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
        }

        return true;
    }

    internal async Task RefundStakesAsync(
        VsMatchSession match)
    {
        foreach (var player in match.Players.Where(player =>
                     player.StakeLocked))
        {
            using var scope = _scopeFactory.CreateScope();
            var cache =
                scope.ServiceProvider
                    .GetRequiredService<IPlayerCacheService>();

            var success = await cache.UpdatePlayerLockedAsync(
                player.PlayerId,
                player.SessionId,
                cachedPlayer =>
                {
                    cachedPlayer.Core.Credit +=
                        match.Classification.Stake;
                    return DirtyFlags.Core;
                },
                CancellationToken.None);

            if (success == true)
            {
                player.StakeLocked = false;
                continue;
            }

            _logger.LogCritical(
                "VS stake refund failed. matchId={MatchId}, playerId={PlayerId}",
                match.MatchId,
                player.PlayerId);
        }
    }

    private async Task<VsMatchPlayerSeed> BuildPlayerSeedAsync(
        VsMatchPlayerState matchPlayer,
        int loadoutSize,
        CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var cache =
            scope.ServiceProvider
                .GetRequiredService<IPlayerCacheService>();

        VsMatchPlayerSeed? seed = null;

        var success = await cache.UpdateQuestionsLockedAsync(
            matchPlayer.PlayerId,
            matchPlayer.SessionId,
            (player, questions) =>
            {
                var selectedSlots =
                    player.BattleTeamSlots.ToArray();

                if (selectedSlots.Length !=
                    matchPlayer.Rounds.Count(round =>
                        !round.IsCaptainRound))
                {
                    return null;
                }

                var characters = selectedSlots
                    .Select(slot => player.Characters[slot - 1])
                    .ToArray();

                if (characters.Any(character =>
                        character is null))
                {
                    return null;
                }

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
                        .. loadout.Take(loadoutSize)
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
                            .Select(question =>
                                new VsOwnQuestionSeed
                                {
                                    QuestionId = question.Id,
                                    Question = question.Question,
                                    CategoryId =
                                        question.CategoryNo,
                                    AnswersJson =
                                        question.AnswersJson
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
            scope.ServiceProvider
                .GetRequiredService<IPlayerCacheService>();

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
}

/**
 * ÚJ FÁJL: a MatchLocked meccs téteinek lefoglalását, a játékos-cache
 * egyszeri snapshotolását, a kérdés-loadout betöltését és
 * infrastruktúrahiba esetén a tétek visszatérítését kezeli. SignalR-
 * üzenetet és fázisváltást nem végez.
 */

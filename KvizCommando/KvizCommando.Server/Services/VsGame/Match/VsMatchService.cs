using KvizCommando.Server.Hubs;
using KvizCommando.Server.Services.VsGame.Matchmaking;
using KvizCommando.Shared.Contracts.VsGame.Match;
using KvizCommando.Shared.Models.Enums.VsGame;
using Microsoft.AspNetCore.SignalR;

namespace KvizCommando.Server.Services.VsGame.Match;

public sealed class VsMatchService : IVsMatchService
{
    private readonly VsMatchStore _store;
    private readonly VsMatchSetupService _setup;
    private readonly IHubContext<VsMatchHub, IVsMatchHubClient> _hub;
    private readonly ILogger<VsMatchService> _logger;

    public VsMatchService(
        VsMatchStore store,
        VsMatchSetupService setup,
        IHubContext<VsMatchHub, IVsMatchHubClient> hub,
        ILogger<VsMatchService> logger)
    {
        _store = store;
        _setup = setup;
        _hub = hub;
        _logger = logger;
    }

    public VsMatchSession LockMatch(
        IReadOnlyList<VsRankedQueueEntry> entries)
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

        AddLog(match, null, "MatchLocked",
            $"Classification={classificationId}");

        if (!_store.TryAdd(match))
        {
            match.Dispose();
            throw new InvalidOperationException(
                "The locked VS match could not be registered.");
        }

        return match;
    }

    public async Task<bool> InitializeLockedMatchAsync(
        VsMatchSession match,
        CancellationToken ct = default)
    {
        try
        {
            await BroadcastMatchAsync(match);

            var hasConnectedPlayer =
                await _setup.InitializePlayersAsync(match, ct);

            (string ConnectionId, VsMatchSnapshot Snapshot)[] messages;

            lock (match.SyncRoot)
            {
                if (match.IsClosed)
                    return false;

                match.IsInitializing = false;
                hasConnectedPlayer =
                    hasConnectedPlayer &&
                    match.Players.Any(player =>
                        player.IsConnected);

                if (!hasConnectedPlayer)
                {
                    messages = [];
                }
                else
                {
                    StartPhaseLocked(
                        match,
                        VsMatchPhase.PreparationOrder);

                    messages =
                        VsMatchSnapshotBuilder.BuildMessages(match);
                }
            }

            if (!hasConnectedPlayer)
            {
                _store.TryRemove(match.MatchId, out _);
                return false;
            }

            await SendBroadcastMessagesAsync(messages);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "VS match initialization failed. matchId={MatchId}",
                match.MatchId);

            string[] connectedClients;

            lock (match.SyncRoot)
            {
                match.IsInitializing = false;
                connectedClients =
                [
                    .. match.Players
                        .Where(player => player.IsConnected)
                        .Select(player => player.ConnectionId)
                ];
            }

            await _setup.RefundStakesAsync(match);
            _store.TryRemove(match.MatchId, out _);

            await SendMatchClosedAsync(
                connectedClients,
                "vsgame.Match.Error.Connection");

            return false;
        }
    }

    public async Task SelectCharacterAsync(
        string connectionId,
        int slotNumber,
        CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        if (!_store.TryGetByConnection(connectionId, out var match) ||
            match is null)
        {
            return;
        }

        (string ConnectionId, VsMatchSnapshot Snapshot)[] messages;

        lock (match.SyncRoot)
        {
            var player = match.FindByConnection(connectionId);
            var target = VsMatchPreparationRules.SelectCharacter(
                match,
                player,
                slotNumber);

            if (target is null)
                return;

            AddLog(
                match,
                player!.PlayerId,
                "PreparationCharacterSelected",
                $"Round={target.RoundNumber};Slot={slotNumber}");

            messages = VsMatchSnapshotBuilder.BuildMessages(match);
        }

        await SendBroadcastMessagesAsync(messages);
    }

    public async Task AssignLoadoutAsync(
        string connectionId,
        VsLoadoutAssignmentRequest request,
        CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        if (!_store.TryGetByConnection(connectionId, out var match) ||
            match is null)
        {
            return;
        }

        (string ConnectionId, VsMatchSnapshot Snapshot)[] messages;

        lock (match.SyncRoot)
        {
            var player = match.FindByConnection(connectionId);
            var target = VsMatchPreparationRules.AssignLoadout(
                match,
                player,
                request.LoadoutPosition,
                request.RoundNumber);

            if (target is null)
                return;

            AddLog(
                match,
                player!.PlayerId,
                "PreparationLoadoutAssigned",
                $"Round={target.RoundNumber};" +
                $"Position={request.LoadoutPosition}");

            messages = VsMatchSnapshotBuilder.BuildMessages(match);
        }

        await SendBroadcastMessagesAsync(messages);
    }

    public async Task AssignHelpAsync(
        string connectionId,
        VsHelpAssignmentRequest request,
        CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        if (!_store.TryGetByConnection(connectionId, out var match) ||
            match is null)
        {
            return;
        }

        (string ConnectionId, VsMatchSnapshot Snapshot)[] messages;

        lock (match.SyncRoot)
        {
            var player = match.FindByConnection(connectionId);
            var target = VsMatchPreparationRules.AssignHelp(
                match,
                player,
                request.HelpType,
                request.RoundNumber);

            if (target is null)
                return;

            AddLog(
                match,
                player!.PlayerId,
                "PreparationHelpAssigned",
                $"Round={target.RoundNumber};Help={request.HelpType}");

            messages = VsMatchSnapshotBuilder.BuildMessages(match);
        }

        await SendBroadcastMessagesAsync(messages);
    }

    public async Task ResetPreparationAsync(
        string connectionId,
        CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        if (!_store.TryGetByConnection(connectionId, out var match) ||
            match is null)
        {
            return;
        }

        (string ConnectionId, VsMatchSnapshot Snapshot)[] messages;

        lock (match.SyncRoot)
        {
            var player = match.FindByConnection(connectionId);

            if (!VsMatchPreparationRules.Reset(match, player))
                return;

            AddLog(
                match,
                player!.PlayerId,
                "PreparationReset",
                string.Empty);

            messages = VsMatchSnapshotBuilder.BuildMessages(match);
        }

        await SendBroadcastMessagesAsync(messages);
    }

    public async Task FinishPreparationAsync(
        string connectionId,
        CancellationToken ct = default)
    {
        if (!_store.TryGetByConnection(connectionId, out var match) ||
            match is null)
        {
            return;
        }

        ct.ThrowIfCancellationRequested();

        (string ConnectionId, VsMatchSnapshot Snapshot)[] messages;

        lock (match.SyncRoot)
        {
            var player = match.FindByConnection(connectionId);

            if (!VsMatchPreparationRules.Finish(match, player))
                return;

            AddLog(
                match,
                player!.PlayerId,
                "PreparationFinished",
                string.Empty);

            AdvanceIfReadyLocked(match);

            messages = VsMatchSnapshotBuilder.BuildMessages(match);
        }

        await SendBroadcastMessagesAsync(messages);
    }

    public async Task SubmitGuessAsync(
        string connectionId,
        VsGuessAnswerRequest request,
        CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        if (!_store.TryGetByConnection(connectionId, out var match) ||
            match is null)
        {
            return;
        }

        (string ConnectionId, VsMatchSnapshot Snapshot)[] messages;

        lock (match.SyncRoot)
        {
            var player = match.FindByConnection(connectionId);
            var receivedUtc = DateTime.UtcNow;

            if (!VsMatchGameRules.SubmitGuess(
                    match,
                    player,
                    request,
                    receivedUtc))
            {
                return;
            }

            AddLog(
                match,
                player!.PlayerId,
                "GuessSubmitted",
                $"Question={request.QuestionNumber}");

            if (VsMatchGameRules
                .HaveAllConnectedPlayersAnswered(match))
            {
                CloseQuestionLocked(match);
            }

            messages =
                VsMatchSnapshotBuilder.BuildMessages(match);
        }

        await SendBroadcastMessagesAsync(messages);
    }

    public async Task SubmitChoiceAsync(
        string connectionId,
        VsChoiceAnswerRequest request,
        CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        if (!_store.TryGetByConnection(connectionId, out var match) ||
            match is null)
        {
            return;
        }

        (string ConnectionId, VsMatchSnapshot Snapshot)[] messages;

        lock (match.SyncRoot)
        {
            var player = match.FindByConnection(connectionId);
            var receivedUtc = DateTime.UtcNow;

            if (!VsMatchGameRules.SubmitChoice(
                    match,
                    player,
                    request,
                    receivedUtc))
            {
                return;
            }

            AddLog(
                match,
                player!.PlayerId,
                "ChoiceSubmitted",
                $"Question={request.QuestionNumber};" +
                $"Answer={request.AnswerIndex}");

            if (VsMatchGameRules
                .HaveAllConnectedPlayersAnswered(match))
            {
                CloseQuestionLocked(match);
            }

            messages =
                VsMatchSnapshotBuilder.BuildMessages(match);
        }

        await SendBroadcastMessagesAsync(messages);
    }

    public async Task SelectCaptainQuestionAsync(
        string connectionId,
        VsCaptainQuestionRequest request,
        CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        if (!_store.TryGetByConnection(connectionId, out var match) ||
            match is null)
        {
            return;
        }

        (string ConnectionId, VsMatchSnapshot Snapshot)[] messages;

        lock (match.SyncRoot)
        {
            var player = match.FindByConnection(connectionId);

            if (!VsMatchGameRules.SelectCaptainQuestion(
                    match,
                    player,
                    request))
            {
                return;
            }

            AddLog(
                match,
                player!.PlayerId,
                "CaptainQuestionSelected",
                $"Position={request.LoadoutPosition}");

            StartPhaseLocked(
                match,
                VsMatchPhase.CaptainQuestion);

            messages =
                VsMatchSnapshotBuilder.BuildMessages(match);
        }

        await SendBroadcastMessagesAsync(messages);
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

        ct.ThrowIfCancellationRequested();

        var removeMatch = false;
        (string ConnectionId, VsMatchSnapshot Snapshot)[] messages = [];

        lock (match.SyncRoot)
        {
            var player = match.FindByConnection(connectionId);

            if (match.IsClosed ||
                player is null ||
                !player.IsConnected)
            {
                return;
            }

            player.IsConnected = false;
            if (IsPreparationPhase(match.Phase))
            {
                VsMatchPreparationRules.ApplyTimeoutDefaults(
                    match,
                    player);
            }

            player.IsFinished = true;

            AddLog(
                match,
                player.PlayerId,
                "Disconnected",
                string.Empty);

            if (IsPreparationPhase(match.Phase))
            {
                AdvanceIfReadyLocked(match);
            }
            else if (IsAnswerPhase(match.Phase) &&
                     VsMatchGameRules
                         .HaveAllConnectedPlayersAnswered(match))
            {
                CloseQuestionLocked(match);
            }

            removeMatch =
                !match.IsInitializing &&
                match.Players.All(item => !item.IsConnected);

            if (!removeMatch &&
                match.Players.Any(item => item.IsConnected))
            {
                messages =
                    VsMatchSnapshotBuilder.BuildMessages(match);
            }
        }

        if (removeMatch)
        {
            _store.TryRemove(match.MatchId, out _);
            return;
        }

        await SendBroadcastMessagesAsync(messages);
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

    private void StartPhaseLocked(
        VsMatchSession match,
        VsMatchPhase phase)
    {
        if (phase == VsMatchPhase.PreparationCompleted)
        {
            AddLog(
                match,
                null,
                "PreparationCompleted",
                string.Empty);

            foreach (var player in match.Players)
                player.IsFinished = !player.IsConnected;

            phase = VsMatchPhase.GameStarting;
        }

        match.PhaseTimerCts.Cancel();
        match.PhaseTimerCts.Dispose();
        match.PhaseTimerCts = new CancellationTokenSource();
        match.Phase = phase;
        match.PhaseStartedUtc = DateTime.UtcNow;

        if (IsPreparationPhase(phase))
        {
            VsMatchPreparationRules.BeginPhase(match, phase);
        }

        if (phase == VsMatchPhase.PreparationHelps &&
            match.Players.All(player => player.IsFinished))
        {
            StartPhaseLocked(
                match,
                VsMatchPhase.PreparationCompleted);
            return;
        }

        var durationSeconds =
            ResolvePhaseDuration(match, phase);

        if (durationSeconds <= 0)
        {
            match.DeadlineUtc = null;
            AddLog(match, null, "PhaseStarted", phase.ToString());
            return;
        }

        match.DeadlineUtc =
            match.PhaseStartedUtc.AddSeconds(durationSeconds);

        AddLog(match, null, "PhaseStarted", phase.ToString());

        _ = RunPhaseTimerAsync(
            match.MatchId,
            match.DeadlineUtc.Value,
            match.PhaseTimerCts.Token);
    }

    private async Task RunPhaseTimerAsync(
        Guid matchId,
        DateTime deadlineUtc,
        CancellationToken ct)
    {
        try
        {
            var delay = deadlineUtc - DateTime.UtcNow;
            if (delay > TimeSpan.Zero)
                await Task.Delay(delay, ct);

            if (ct.IsCancellationRequested)
                return;

            if (!_store.TryGet(matchId, out var match) ||
                match is null)
            {
                return;
            }

            (string ConnectionId, VsMatchSnapshot Snapshot)[] messages;

            lock (match.SyncRoot)
            {
                if (ct.IsCancellationRequested ||
                    match.IsClosed)
                    return;

                if (IsPreparationPhase(match.Phase) &&
                    match.Profile.PausePreparationOnTimeout)
                {
                    AddLog(
                        match,
                        null,
                        "PreparationTimerPaused",
                        match.Phase.ToString());
                    return;
                }

                HandlePhaseTimeoutLocked(match);

                messages =
                    VsMatchSnapshotBuilder.BuildMessages(match);
            }

            await SendBroadcastMessagesAsync(messages);
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
        var nextPhase =
            VsMatchPreparationRules.GetNextPhase(match);

        if (nextPhase.HasValue)
            StartPhaseLocked(match, nextPhase.Value);
    }

    private void HandlePhaseTimeoutLocked(
        VsMatchSession match)
    {
        switch (match.Phase)
        {
            case VsMatchPhase.PreparationOrder:
            case VsMatchPhase.PreparationCategories:
            case VsMatchPhase.PreparationHelps:
                FinishPreparationPhaseLocked(match);
                break;

            case VsMatchPhase.GameStarting:
                VsMatchGameRules.BeginFirstNormalRound(match);
                StartPhaseLocked(
                    match,
                    VsMatchPhase.NormalRoundGuess);
                break;

            case VsMatchPhase.NormalRoundGuess:
            case VsMatchPhase.NormalRoundQuestion:
            case VsMatchPhase.CaptainQuestion:
                CloseQuestionLocked(match);
                break;

            case VsMatchPhase.QuestionResult:
                ContinueAfterQuestionResultLocked(match);
                break;

            case VsMatchPhase.NormalRoundResult:
                ContinueAfterNormalRoundLocked(match);
                break;

            case VsMatchPhase.CaptainQuestionSelection:
                VsMatchGameRules
                    .SelectDefaultCaptainQuestion(match);
                StartPhaseLocked(
                    match,
                    VsMatchPhase.CaptainQuestion);
                break;

            case VsMatchPhase.CaptainRoundResult:
                VsMatchGameRules.CommitRoundResult(match);
                StartPhaseLocked(
                    match,
                    VsMatchPhase.GameCompleted);
                break;
        }
    }

    private void FinishPreparationPhaseLocked(
        VsMatchSession match)
    {
        foreach (var player in match.Players
                     .Where(player => !player.IsFinished))
        {
            VsMatchPreparationRules.ApplyTimeoutDefaults(
                match,
                player);
            player.IsFinished = true;
            AddLog(
                match,
                player.PlayerId,
                "PreparationTimeout",
                match.Phase.ToString());
        }

        AdvanceIfReadyLocked(match);
    }

    private void CloseQuestionLocked(VsMatchSession match)
    {
        VsMatchGameRules.CloseCurrentQuestion(match);

        AddLog(
            match,
            null,
            "QuestionClosed",
            $"Question={match.Game.QuestionNumber}");

        StartPhaseLocked(
            match,
            VsMatchPhase.QuestionResult);
    }

    private void ContinueAfterQuestionResultLocked(
        VsMatchSession match)
    {
        if (match.Game.QuestionKind ==
            VsQuestionKind.Guess)
        {
            VsMatchGameRules.BeginNormalQuestion(match);
            StartPhaseLocked(
                match,
                VsMatchPhase.NormalRoundQuestion);
            return;
        }

        var isCaptainRound =
            match.Game.CurrentRoundNumber >
            match.Classification.RequiredPartySize;

        if (!isCaptainRound &&
            VsMatchGameRules.HasNextNormalQuestion(match))
        {
            VsMatchGameRules.MoveToNextNormalQuestion(match);
            StartPhaseLocked(
                match,
                VsMatchPhase.NormalRoundQuestion);
            return;
        }

        if (!isCaptainRound)
        {
            VsMatchGameRules.BuildNormalRoundResult(match);
            StartPhaseLocked(
                match,
                VsMatchPhase.NormalRoundResult);
            return;
        }

        if (VsMatchGameRules.HasNextCaptainQuestion(match))
        {
            VsMatchGameRules
                .MoveToNextCaptainSelection(match);
            StartPhaseLocked(
                match,
                VsMatchPhase.CaptainQuestionSelection);
            return;
        }

        VsMatchGameRules.BuildCaptainRoundResult(match);
        StartPhaseLocked(
            match,
            VsMatchPhase.CaptainRoundResult);
    }

    private void ContinueAfterNormalRoundLocked(
        VsMatchSession match)
    {
        VsMatchGameRules.CommitRoundResult(match);

        if (VsMatchGameRules.HasNextNormalRound(match))
        {
            VsMatchGameRules.BeginNextNormalRound(match);
            StartPhaseLocked(
                match,
                VsMatchPhase.NormalRoundGuess);
            return;
        }

        VsMatchGameRules.BeginCaptainRound(match);
        StartPhaseLocked(
            match,
            VsMatchPhase.CaptainQuestionSelection);
    }

    private static int ResolvePhaseDuration(
        VsMatchSession match,
        VsMatchPhase phase) =>
        phase switch
        {
            VsMatchPhase.PreparationOrder or
            VsMatchPhase.PreparationCategories or
            VsMatchPhase.PreparationHelps =>
                match.Profile.PreparationSeconds,

            VsMatchPhase.GameStarting =>
                match.Profile.PhasePauseSeconds,

            VsMatchPhase.NormalRoundGuess =>
                match.Profile.GuessSeconds,

            VsMatchPhase.NormalRoundQuestion or
            VsMatchPhase.CaptainQuestion =>
                match.Profile.QuestionSeconds,

            VsMatchPhase.QuestionResult =>
                match.Profile.QuestionPauseSeconds,

            VsMatchPhase.NormalRoundResult or
            VsMatchPhase.CaptainRoundResult =>
                match.Profile.RoundResultSeconds,

            VsMatchPhase.CaptainQuestionSelection =>
                match.Profile.PhasePauseSeconds,

            _ => 0
        };

    private static bool IsPreparationPhase(
        VsMatchPhase phase) =>
        phase is
            VsMatchPhase.PreparationOrder or
            VsMatchPhase.PreparationCategories or
            VsMatchPhase.PreparationHelps;

    private static bool IsAnswerPhase(
        VsMatchPhase phase) =>
        phase is
            VsMatchPhase.NormalRoundGuess or
            VsMatchPhase.NormalRoundQuestion or
            VsMatchPhase.CaptainQuestion;

    private async Task BroadcastMatchAsync(
        VsMatchSession match)
    {
        (string ConnectionId, VsMatchSnapshot Snapshot)[] messages;

        lock (match.SyncRoot)
        {
            if (match.IsClosed)
                return;

            messages =
                VsMatchSnapshotBuilder.BuildMessages(match);
        }

        await SendBroadcastMessagesAsync(messages);
    }

    private async Task SendBroadcastMessagesAsync(
        (string ConnectionId, VsMatchSnapshot Snapshot)[] messages)
    {
        foreach (var message in messages)
        {
            await _hub.Clients
                .Client(message.ConnectionId)
                .MatchChanged(message.Snapshot);
        }
    }

    private async Task SendMatchClosedAsync(
        IEnumerable<string> connectionIds,
        string messageKey)
    {
        foreach (var connectionId in connectionIds)
        {
            await _hub.Clients
                .Client(connectionId)
                .MatchClosed(messageKey);
        }
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

/**
 * MÓDOSÍTÁS: a preparáció feltételei és állapotmódosításai a
 * VsMatchPreparationRules osztályba kerültek. A service publikus
 * műveletei rövid, explicit koordinátorok maradtak: sessionkeresés,
 * lock, naplózás, fázisváltás és snapshot-küldés. A fázis időzítőjét
 * kizárólag a saját CancellationTokenje érvényteleníti; technikai
 * PhaseVersion és az egyszer használatos ToQueue segéd megszűnt.
 * Az in-memory állapot csak await nélküli lock alatt változik, a
 * SignalR-küldés a lockon kívül történik.
 *
 * MÓDOSÍTÁS: ugyanazzal az egy szerveridőzítővel koordinálja a tipp-,
 * normál-, eredmény- és kapitányfázisokat, miközben a szabályokat és
 * a pontozást a két statikus domain-segédben hagyja.
 *
 * A fájl a MatchLocked session létrehozását, a parancsok authoritative
 * feldolgozását, a fázisváltást, a szerverórát és a disconnect miatti
 * takarítást koordinálja.
 */

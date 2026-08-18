using KvizCommando.Shared.Contracts.VsGame.Match;

namespace KvizCommando.Server.Services.VsGame.Match;

public sealed partial class VsMatchService
{
    /// <inheritdoc />
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

    /// <inheritdoc />
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

            AddLog( match, player!.PlayerId, "PreparationLoadoutAssigned", $"Round={target.RoundNumber};" + $"Position={request.LoadoutPosition}");

            messages = VsMatchSnapshotBuilder.BuildMessages(match);
        }

        await SendBroadcastMessagesAsync(messages);
    }

    /// <inheritdoc />
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

    /// <inheritdoc />
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

    /// <inheritdoc />
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
}

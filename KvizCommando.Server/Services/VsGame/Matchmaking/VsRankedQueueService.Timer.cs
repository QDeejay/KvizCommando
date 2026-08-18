using KvizCommando.Server.Services.VsGame.Match;

namespace KvizCommando.Server.Services.VsGame.Matchmaking;

public sealed partial class VsRankedQueueService
{
    private bool IsReentryBlocked(int playerId)
    {
        lock (_syncRoot)
        {
            if (!_reentryBlockedUntilUtc.TryGetValue(
                    playerId,
                    out var blockedUntilUtc))
            {
                return false;
            }

            if (blockedUntilUtc > DateTime.UtcNow)
                return true;

            _reentryBlockedUntilUtc.Remove(playerId);
            return false;
        }
    }

    private void UpdateMatchmakingTimerAfterJoinLocked(
        VsRankedQueueState queue,
        int previousCount)
    {
        if (queue.Entries.Count <
            VsRankedQueueRules.MINIMUM_PLAYERS)
            return;

        var nowUtc = DateTime.UtcNow;

        if (!queue.MatchmakingDeadlineUtc.HasValue)
        {
            queue.ArrivalExtensionUsed = false;
            queue.MatchmakingDeadlineUtc =
                nowUtc.AddSeconds(
                    VsRankedQueueRules.INITIAL_WAIT_SECONDS);
            return;
        }

        if (previousCount <
                VsRankedQueueRules.MINIMUM_PLAYERS ||
            queue.ArrivalExtensionUsed)
        {
            return;
        }

        queue.ArrivalExtensionUsed = true;

        var extendedDeadlineUtc =
            queue.MatchmakingDeadlineUtc.Value.AddSeconds(
                VsRankedQueueRules
                    .THIRD_PLAYER_EXTENSION_MAX_SECONDS);
        var maximumDeadlineUtc =
            nowUtc.AddSeconds(
                VsRankedQueueRules.INITIAL_WAIT_SECONDS);

        queue.MatchmakingDeadlineUtc =
            extendedDeadlineUtc < maximumDeadlineUtc
                ? extendedDeadlineUtc
                : maximumDeadlineUtc;
    }

    private void UpdateMatchmakingTimerAfterLeaveLocked(
        VsRankedQueueState queue)
    {
        if (queue.Entries.Count <
            VsRankedQueueRules.MINIMUM_PLAYERS)
        {
            ClearMatchmakingTimerLocked(queue);
        }
    }

    private static void ClearMatchmakingTimerLocked(
        VsRankedQueueState queue)
    {
        queue.MatchmakingDeadlineUtc = null;
        queue.ArrivalExtensionUsed = false;
    }

    private async Task RunMatchmakingLoopAsync(CancellationToken ct)
    {
        using var timer = new PeriodicTimer(
            TimeSpan.FromMilliseconds(
                VsRankedQueueRules.TIMER_INTERVAL_MS));

        try
        {
            while (await timer.WaitForNextTickAsync(ct))
            {
                int[] expiredClassifications;

                lock (_syncRoot)
                {
                    var nowUtc = DateTime.UtcNow;

                    expiredClassifications =
                    [
                        .. _queues
                            .Where(queue =>
                                queue.Value.MatchmakingDeadlineUtc
                                    .HasValue &&
                                queue.Value.MatchmakingDeadlineUtc
                                    .Value <= nowUtc)
                            .Select(queue => queue.Key)
                    ];

                    foreach (var playerId in _reentryBlockedUntilUtc
                                 .Where(block => block.Value <= nowUtc)
                                 .Select(block => block.Key)
                                 .ToArray())
                    {
                        _reentryBlockedUntilUtc.Remove(playerId);
                    }
                }

                foreach (var classificationId in expiredClassifications)
                {
                    try
                    {
                        await LockTimedMatchAsync(classificationId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(
                            ex,
                            "Dynamic VS matchmaking failed. classificationId={ClassificationId}",
                            classificationId);
                    }
                }
            }
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
        }
    }

    private async Task LockTimedMatchAsync(int classificationId)
    {
        VsMatchSession lockedMatch;

        lock (_syncRoot)
        {
            var queue = _queues[classificationId];

            if (!queue.MatchmakingDeadlineUtc.HasValue ||
                queue.MatchmakingDeadlineUtc.Value > DateTime.UtcNow ||
                queue.Entries.Count <
                    VsRankedQueueRules.MINIMUM_PLAYERS)
            {
                return;
            }

            lockedMatch = _matchService.LockMatch(
                queue.Entries.ToArray());
            queue.Entries.Clear();
            ClearMatchmakingTimerLocked(queue);
        }

        await BroadcastQueueAsync(classificationId);
        await _matchService.InitializeLockedMatchAsync(
            lockedMatch,
            CancellationToken.None);
    }

    /// <summary>
    /// Aszinkron módon felszabadítja a példány által használt erőforrásokat.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        _lifetimeCts.Cancel();
        await _matchmakingLoop;
        _lifetimeCts.Dispose();
        GC.SuppressFinalize(this);
    }
}

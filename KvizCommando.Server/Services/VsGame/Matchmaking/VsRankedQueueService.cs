using KvizCommando.Server.Hubs;
using KvizCommando.Server.Services.PlayerCache;
using KvizCommando.Server.Services.VsGame.Match;
using KvizCommando.Shared.Contracts.VsGame.Match;
using KvizCommando.Shared.Models;
using KvizCommando.Shared.Models.Dtos;
using KvizCommando.Shared.Models.Enums.VsGame;
using KvizCommando.Shared.Models.Rules;
using Microsoft.AspNetCore.SignalR;
using System.Text.Json;

namespace KvizCommando.Server.Services.VsGame.Matchmaking;

public sealed partial class VsRankedQueueService :
    IVsRankedQueueService,
    IAsyncDisposable
{
    private readonly object _syncRoot = new();
    private readonly Dictionary<int, VsRankedQueueState> _queues =
        VsBattleClassificationRules.List.ToDictionary(
            rule => rule.ClassificationId,
            _ => new VsRankedQueueState());
    private readonly Dictionary<int, DateTime> _reentryBlockedUntilUtc = [];
    private readonly CancellationTokenSource _lifetimeCts = new();

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly VsMatchStore _matchStore;
    private readonly IVsMatchService _matchService;
    private readonly IHubContext<VsMatchHub, IVsMatchHubClient> _hub;
    private readonly ILogger<VsRankedQueueService> _logger;
    private readonly Task _matchmakingLoop;

    public VsRankedQueueService(
        IServiceScopeFactory scopeFactory,
        VsMatchStore matchStore,
        IVsMatchService matchService,
        IHubContext<VsMatchHub, IVsMatchHubClient> hub,
        ILogger<VsRankedQueueService> logger)
    {
        _scopeFactory = scopeFactory;
        _matchStore = matchStore;
        _matchService = matchService;
        _hub = hub;
        _logger = logger;
        _matchmakingLoop = RunMatchmakingLoopAsync(
            _lifetimeCts.Token);
    }

    public IReadOnlyDictionary<int, int>
        GetConnectedPlayerCounts()
    {
        lock (_syncRoot)
        {
            var playersByClassification =
                VsBattleClassificationRules.List.ToDictionary(
                    rule => rule.ClassificationId,
                    _ => new HashSet<int>());

            foreach (var queue in _queues)
            {
                foreach (var entry in queue.Value.Entries)
                    playersByClassification[queue.Key]
                        .Add(entry.PlayerId);
            }

            foreach (var player in _matchStore.GetConnectedPlayers())
            {
                if (playersByClassification.TryGetValue(
                        player.ClassificationId,
                        out var players))
                {
                    players.Add(player.PlayerId);
                }
            }

            return playersByClassification.ToDictionary(
                item => item.Key,
                item => item.Value.Count);
        }
    }

    /// <inheritdoc />
    public async Task<VsQueueJoinResult> JoinAsync(
        int playerId,
        string sessionId,
        string connectionId,
        int classificationId,
        int responseTimeMilliseconds,
        VsConnectionQuality connectionQuality,
        CancellationToken ct = default)
    {
        var rule = VsBattleClassificationRules.List.FirstOrDefault(
            item => item.ClassificationId == classificationId);

        if (rule is null)
        {
            return new VsQueueJoinResult
            {
                ErrorKey =
                    "vsgame.Match.Error.Classification"
            };
        }

        if (_matchStore.ContainsPlayer(playerId))
        {
            return new VsQueueJoinResult
            {
                ErrorKey =
                    "vsgame.Match.Error.AlreadyLocked"
            };
        }

        if (IsReentryBlocked(playerId))
        {
            return new VsQueueJoinResult
            {
                ErrorKey =
                    "vsgame.Match.Error.QueueCooldown"
            };
        }

        var entry = await BuildQueueEntryAsync(
            playerId,
            sessionId,
            connectionId,
            rule,
            responseTimeMilliseconds,
            connectionQuality,
            ct);

        if (entry is null)
        {
            return new VsQueueJoinResult
            {
                ErrorKey =
                    "vsgame.Match.Error.QueueValidation"
            };
        }

        VsMatchSession? lockedMatch = null;
        VsQueueJoinResult result;

        ct.ThrowIfCancellationRequested();

        lock (_syncRoot)
        {
            if (_matchStore.ContainsPlayer(playerId))
            {
                result = new VsQueueJoinResult
                {
                    ErrorKey =
                        "vsgame.Match.Error.AlreadyLocked"
                };
            }
            else
            {
                RemoveExistingEntry(playerId, connectionId);
                var queue = _queues[classificationId];
                var previousCount = queue.Entries.Count;
                queue.Entries.Add(entry);

                if (queue.Entries.Count >=
                    VsRankedQueueRules.REQUIRED_PLAYERS)
                {
                    List<VsRankedQueueEntry> matchedEntries =
                    [
                        .. queue.Entries
                            .Take(VsRankedQueueRules.REQUIRED_PLAYERS)
                    ];

                    lockedMatch =
                        _matchService.LockMatch(matchedEntries);

                    queue.Entries.RemoveRange(
                        0,
                        matchedEntries.Count);
                    ClearMatchmakingTimerLocked(queue);
                }
                else
                {
                    UpdateMatchmakingTimerAfterJoinLocked(
                        queue,
                        previousCount);
                }

                result = new VsQueueJoinResult
                {
                    IsAccepted = true
                };
            }
        }

        await BroadcastQueueAsync(classificationId);

        if (lockedMatch is not null)
        {
            var initialized =
                await _matchService.InitializeLockedMatchAsync(
                    lockedMatch,
                    CancellationToken.None);

            if (!initialized)
            {
                result = new VsQueueJoinResult
                {
                    ErrorKey =
                        "vsgame.Match.Error.Connection"
                };
            }
        }

        return result;
    }

    /// <inheritdoc />
    public Task<VsQueueLeaveStatus> LeaveAsync(
        string connectionId,
        CancellationToken ct = default) =>
        RemoveEntryAsync(
            entry => entry.ConnectionId == connectionId,
            ct);

    /// <inheritdoc />
    public async Task DisconnectAsync(
        string connectionId,
        CancellationToken ct = default)
    {
        await RemoveEntryAsync(
            entry => entry.ConnectionId == connectionId,
            ct);
    }

    /// <inheritdoc />
    public async Task LeavePlayerAsync(
        int playerId,
        CancellationToken ct = default)
    {
        await RemoveEntryAsync(
            entry => entry.PlayerId == playerId,
            ct);
    }

    private async Task<VsQueueLeaveStatus> RemoveEntryAsync(
        Func<VsRankedQueueEntry, bool> matches,
        CancellationToken ct)
    {
        int? changedClassification = null;
        var isReentryBlocked = false;

        ct.ThrowIfCancellationRequested();

        lock (_syncRoot)
        {
            foreach (var queue in _queues)
            {
                var entry = queue.Value.Entries.FirstOrDefault(matches);

                if (entry is null)
                    continue;

                var hadOtherWaitingPlayers =
                    queue.Value.Entries.Count > 1;

                queue.Value.Entries.Remove(entry);
                changedClassification = queue.Key;
                UpdateMatchmakingTimerAfterLeaveLocked(queue.Value);

                if (hadOtherWaitingPlayers)
                {
                    _reentryBlockedUntilUtc[entry.PlayerId] =
                        DateTime.UtcNow.AddSeconds(
                            VsRankedQueueRules.REENTRY_BLOCK_SECONDS);
                    isReentryBlocked = true;
                }

                break;
            }
        }

        if (changedClassification.HasValue)
            await BroadcastQueueAsync(changedClassification.Value);

        if (!changedClassification.HasValue)
            return VsQueueLeaveStatus.NotInQueue;

        return isReentryBlocked
            ? VsQueueLeaveStatus.LeftWithCooldown
            : VsQueueLeaveStatus.Left;
    }

    private async Task<VsRankedQueueEntry?> BuildQueueEntryAsync(
        int playerId,
        string sessionId,
        string connectionId,
        VsBattleClassificationDto rule,
        int responseTimeMilliseconds,
        VsConnectionQuality connectionQuality,
        CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var cache =
            scope.ServiceProvider.GetRequiredService<IPlayerCacheService>();

        VsRankedQueueEntry? result = null;

        var success = await cache.UpdateQuestionsLockedAsync(
            playerId,
            sessionId,
            (player, questions) =>
            {
                var selectedSlots = player.BattleTeamSlots;

                if (selectedSlots.Length != rule.RequiredPartySize ||
                    selectedSlots.Any(slot => slot is < 1 or > 8) ||
                    selectedSlots.Distinct().Count() !=
                        selectedSlots.Length ||
                    player.Core.Credit < rule.Stake)
                {
                    return null;
                }

                var selectedMembers = selectedSlots
                    .Select(slot => player.Characters[slot - 1])
                    .ToArray();

                if (selectedMembers.Any(member =>
                        member is null ||
                        !VsBattleClassificationRules.CanSelectMember(
                            player.Core.RankEnum,
                            member.EnergyPoints,
                            member.Rank,
                            member.XP)))
                {
                    return null;
                }

                var eligibleIds =
                    VsBattleClassificationRules
                        .GetEligibleClassificationIds(
                            player.Core.RankEnum,
                            selectedMembers
                                .Select(member => member!.Rank)
                                .ToArray());

                if (!eligibleIds.Contains(rule.ClassificationId))
                    return null;

                var loadout = JsonSerializer.Deserialize<int[]>(
                                  player.Loadout.FactorySlotsJson) ??
                              [];
                var maxUserSlot = Math.Min(
                    RankRewards.List[player.Core.RankEnum].OwnQuestSlot,
                    questions.uSlots.Length);

                if (!HasValidLoadout(
                        loadout,
                        player.Core.RankEnum,
                        rule.RequiredPartySize,
                        questions.uSlots
                            .Take(maxUserSlot)
                            .Count(question =>
                                question is not null &&
                                question.CategoryNo > 0)))
                {
                    return null;
                }

                result = new VsRankedQueueEntry
                {
                    PlayerId = playerId,
                    SessionId = sessionId,
                    ConnectionId = connectionId,
                    ClassificationId = rule.ClassificationId,
                    DisplayName = player.Core.DisplayName,
                    TeamName = player.Core.TeamName,
                    TeamLevel = player.Core.RankEnum,
                    ResponseTimeMilliseconds =
                        responseTimeMilliseconds,
                    ConnectionQuality = connectionQuality
                };

                return 0u;
            },
            ct);

        return success == CacheUpdateResult.Updated ? result : null;
    }

    private static bool HasValidLoadout(
        int[] loadout,
        int teamLevel,
        int requiredPartySize,
        int availableOwnQuestions)
    {
        var loadoutSize =
            QuestionLoadoutRules.GetLoadoutSize(teamLevel);

        if (loadout.Length < loadoutSize)
            return false;

        var matchLoadout = loadout
            .Take(loadoutSize)
            .ToArray();

        if (matchLoadout.Any(category =>
                category !=
                    VsLoadoutCategoryIds.AllCategories &&
                (category <
                    VsLoadoutCategoryIds.MinimumFactoryCategory ||
                 category >
                    VsLoadoutCategoryIds.OwnQuestion)))
        {
            return false;
        }

        if (matchLoadout
            .Skip(loadoutSize / 2)
            .Any(category =>
                category == VsLoadoutCategoryIds.OwnQuestion))
        {
            return false;
        }

        var ownQuestionCount = matchLoadout.Count(category =>
            category == VsLoadoutCategoryIds.OwnQuestion);
        var ownQuestionLimit =
            QuestionLoadoutRules.GetOwnQuestionLimit(
                loadoutSize,
                availableOwnQuestions);

        return
            matchLoadout.Count(category =>
                category !=
                    VsLoadoutCategoryIds.OwnQuestion) >=
            requiredPartySize &&
            ownQuestionCount <= ownQuestionLimit;
    }

    private void RemoveExistingEntry(
        int playerId,
        string connectionId)
    {
        foreach (var queue in _queues.Values)
        {
            var removed = queue.Entries.RemoveAll(entry =>
                entry.PlayerId == playerId ||
                entry.ConnectionId == connectionId);

            if (removed > 0)
                UpdateMatchmakingTimerAfterLeaveLocked(queue);
        }
    }

    private async Task BroadcastQueueAsync(int classificationId)
    {
        (string ConnectionId, VsRankedQueueSnapshot Snapshot)[] messages;

        lock (_syncRoot)
        {
            messages = BuildQueueMessagesLocked(classificationId);
        }

        foreach (var message in messages)
        {
            await _hub.Clients
                .Client(message.ConnectionId)
                .QueueChanged(message.Snapshot);
        }
    }

    private (
        string ConnectionId,
        VsRankedQueueSnapshot Snapshot)[] BuildQueueMessagesLocked(
            int classificationId)
    {
        var queue = _queues[classificationId];
        var entries = queue.Entries.ToArray();
        var rule = VsBattleClassificationRules.List.First(
            item => item.ClassificationId == classificationId);

        return
        [
            .. entries.Select(currentEntry => (
                currentEntry.ConnectionId,
                new VsRankedQueueSnapshot
                {
                    ClassificationId = classificationId,
                    WaitingPlayers = entries.Length,
                    RequiredPlayers =
                        VsRankedQueueRules.REQUIRED_PLAYERS,
                    RequiredPartySize = rule.RequiredPartySize,
                    Stake = rule.Stake,
                    MatchmakingDeadlineUtc =
                        queue.MatchmakingDeadlineUtc,
                    Players =
                    [
                        .. entries.Select((entry, index) =>
                            new VsMatchPlayerDto
                            {
                                Position = index + 1,
                                DisplayName = entry.DisplayName,
                                TeamName = entry.TeamName,
                                TeamLevel = entry.TeamLevel,
                                ResponseTimeMilliseconds =
                                    entry.ResponseTimeMilliseconds,
                                ConnectionQuality =
                                    entry.ConnectionQuality,
                                IsMe =
                                    entry.PlayerId ==
                                    currentEntry.PlayerId,
                                IsConnected = true
                            })
                    ]
                }))
        ];
    }

}

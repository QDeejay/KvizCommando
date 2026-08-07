using KvizCommando.Server.Hubs;
using KvizCommando.Server.Services.PlayerCache;
using KvizCommando.Server.Services.VsGame.Match;
using KvizCommando.Shared.Contracts.VsGame.Match;
using KvizCommando.Shared.Models.Dtos;
using KvizCommando.Shared.Models.Enums.VsGame;
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
                    VsMatchProfiles.Ranked.RequiredPlayers)
                {
                    List<VsRankedQueueEntry> matchedEntries =
                    [
                        .. queue.Entries
                            .Take(VsMatchProfiles.Ranked.RequiredPlayers)
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

    public Task<bool> LeaveAsync(
        string connectionId,
        CancellationToken ct = default) =>
        RemoveConnectionAsync(
            connectionId,
            applyReentryBlock: true,
            ct);

    public async Task DisconnectAsync(
        string connectionId,
        CancellationToken ct = default)
    {
        await RemoveConnectionAsync(
            connectionId,
            applyReentryBlock: false,
            ct);
    }

    public async Task LeavePlayerAsync(
        int playerId,
        string sessionId,
        CancellationToken ct = default)
    {
        int? changedClassification = null;

        ct.ThrowIfCancellationRequested();

        lock (_syncRoot)
        {
            foreach (var queue in _queues)
            {
                var removed = queue.Value.Entries.RemoveAll(entry =>
                    entry.PlayerId == playerId &&
                    entry.SessionId == sessionId);

                if (removed > 0)
                {
                    changedClassification = queue.Key;
                    UpdateMatchmakingTimerAfterLeaveLocked(
                        queue.Value);
                    break;
                }
            }
        }

        if (changedClassification.HasValue)
            await BroadcastQueueAsync(changedClassification.Value);
    }

    private async Task<bool> RemoveConnectionAsync(
        string connectionId,
        bool applyReentryBlock,
        CancellationToken ct)
    {
        int? changedClassification = null;

        ct.ThrowIfCancellationRequested();

        lock (_syncRoot)
        {
            foreach (var queue in _queues)
            {
                var entry = queue.Value.Entries.FirstOrDefault(item =>
                    item.ConnectionId == connectionId);

                if (entry is null)
                    continue;

                queue.Value.Entries.Remove(entry);
                changedClassification = queue.Key;
                UpdateMatchmakingTimerAfterLeaveLocked(queue.Value);

                if (applyReentryBlock)
                {
                    _reentryBlockedUntilUtc[entry.PlayerId] =
                        DateTime.UtcNow.AddSeconds(
                            VsMatchProfiles.Ranked
                                .QueueReentryBlockSeconds);
                }

                break;
            }
        }

        if (changedClassification.HasValue)
            await BroadcastQueueAsync(changedClassification.Value);

        return changedClassification.HasValue;
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

                if (!HasValidLoadout(
                        loadout,
                        rule.RequiredPartySize,
                        questions.uSlots.Count(question =>
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

        return success == true ? result : null;
    }

    private static bool HasValidLoadout(
        int[] loadout,
        int requiredPartySize,
        int availableOwnQuestions)
    {
        if (loadout.Length < VsMatchProfiles.Ranked.LoadoutSize)
            return false;

        var matchLoadout = loadout
            .Take(VsMatchProfiles.Ranked.LoadoutSize)
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

        return
            matchLoadout.Count(category =>
                category !=
                VsLoadoutCategoryIds.OwnQuestion) >=
            requiredPartySize &&
            matchLoadout.Count(category =>
                category ==
                VsLoadoutCategoryIds.OwnQuestion) <=
            availableOwnQuestions;
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
                        VsMatchProfiles.Ranked.RequiredPlayers,
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

/**
 * MÓDOSÍTÁS: a queue szinkron lockot használ, mert a kritikus
 * szakaszban nincs await. A kiválasztott játékosok MatchLocked
 * sessionje még a queue lock elengedése előtt bekerül a store-ba, így
 * megszűnik a queue és a match közötti disconnect-rés és nincs szükség
 * _lockingPlayers segédállapotra. A belépés közvetlen eredményt ad.
 * A DTO-pillanatkép a várólisták és a kapcsolódott meccsjátékosok
 * PlayerId-halmazának unióját számolja, ezért átmozgatáskor sincs
 * kettős számlálás.
 * MÓDOSÍTÁS: queue-belépéskor a nyugdíjazási XP-határt is ugyanazzal
 * a központi karakterválaszthatósági szabállyal validálja.
 * MÓDOSÍTÁS: a Hub által mért kapcsolati adat a queue entryből minden
 * várakozó címzett publikus roster-snapshotjába bekerül.
 * MÓDOSÍTÁS: logoutkor PlayerId és SessionId alapján azonnal törli a
 * várakozót és kiküldi a friss queue-snapshotot.
 * MÓDOSÍTÁS: a második játékos 60 másodperces szerverhatáridőt indít.
 * Az első további érkező egyszer, legfeljebb 60 másodpercig hosszabbít;
 * kilépéskor az óra csak egy főre visszaesve szűnik meg. Egyetlen közös
 * PeriodicTimer figyeli mind az öt besorolást, valamint a manuális
 * kilépés egyperces újrabelépési tiltását. A SignalR-disconnect és a
 * logout büntetésmentes marad.
 *
 * Az öt besorolás külön várólistáját kezeli, cache-snapshotból
 * validálja a belépést, majd négy játékosnál azonnal, a határidő
 * lejártakor pedig az aktuális 2–3 játékossal MatchLocked meccset hoz
 * létre. Kérdést nem tölt és játékállapotot nem tárol.
 */

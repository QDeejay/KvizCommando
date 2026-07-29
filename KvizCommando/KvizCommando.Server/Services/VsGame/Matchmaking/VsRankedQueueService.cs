using KvizCommando.Server.Hubs;
using KvizCommando.Server.Services.PlayerCache;
using KvizCommando.Server.Services.VsGame.Match;
using KvizCommando.Shared.Contracts.VsGame.Match;
using KvizCommando.Shared.Models.Dtos;
using KvizCommando.Shared.Models.Enums.VsGame;
using Microsoft.AspNetCore.SignalR;
using System.Text.Json;

namespace KvizCommando.Server.Services.VsGame.Matchmaking;

public sealed class VsRankedQueueService : IVsRankedQueueService
{
    private readonly object _syncRoot = new();
    private readonly Dictionary<int, List<VsRankedQueueEntry>> _queues =
        VsBattleClassificationRules.List.ToDictionary(
            rule => rule.ClassificationId,
            _ => new List<VsRankedQueueEntry>());

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly VsMatchStore _matchStore;
    private readonly IVsMatchService _matchService;
    private readonly IHubContext<VsMatchHub, IVsMatchHubClient> _hub;

    public VsRankedQueueService(
        IServiceScopeFactory scopeFactory,
        VsMatchStore matchStore,
        IVsMatchService matchService,
        IHubContext<VsMatchHub, IVsMatchHubClient> hub)
    {
        _scopeFactory = scopeFactory;
        _matchStore = matchStore;
        _matchService = matchService;
        _hub = hub;
    }

    public async Task<VsQueueJoinResult> JoinAsync(
        int playerId,
        string sessionId,
        string connectionId,
        int classificationId,
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

        var entry = await BuildQueueEntryAsync(
            playerId,
            sessionId,
            connectionId,
            rule,
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
                _queues[classificationId].Add(entry);

                if (_queues[classificationId].Count >=
                    VsMatchProfiles.Ranked.RequiredPlayers)
                {
                    List<VsRankedQueueEntry> matchedEntries =
                    [
                        .. _queues[classificationId]
                            .Take(VsMatchProfiles.Ranked.RequiredPlayers)
                    ];

                    lockedMatch =
                        _matchService.LockMatch(matchedEntries);

                    _queues[classificationId].RemoveRange(
                        0,
                        matchedEntries.Count);
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

    public async Task LeaveAsync(
        string connectionId,
        CancellationToken ct = default)
    {
        int? changedClassification = null;

        ct.ThrowIfCancellationRequested();

        lock (_syncRoot)
        {
            foreach (var queue in _queues)
            {
                var removed = queue.Value.RemoveAll(
                    entry => entry.ConnectionId == connectionId);

                if (removed > 0)
                {
                    changedClassification = queue.Key;
                    break;
                }
            }
        }

        if (changedClassification.HasValue)
            await BroadcastQueueAsync(changedClassification.Value);
    }

    private async Task<VsRankedQueueEntry?> BuildQueueEntryAsync(
        int playerId,
        string sessionId,
        string connectionId,
        VsBattleClassificationDto rule,
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
                            member.Rank)))
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
                    TeamLevel = player.Core.RankEnum
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
            queue.RemoveAll(entry =>
                entry.PlayerId == playerId ||
                entry.ConnectionId == connectionId);
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
        var entries = _queues[classificationId].ToArray();
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
                    Players =
                    [
                        .. entries.Select((entry, index) =>
                            new VsMatchPlayerDto
                            {
                                Position = index + 1,
                                DisplayName = entry.DisplayName,
                                TeamName = entry.TeamName,
                                TeamLevel = entry.TeamLevel,
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
 *
 * Az öt besorolás külön várólistáját kezeli, cache-snapshotból
 * validálja a belépést, majd a profil szerinti játékosszámnál
 * MatchLocked meccset hoz létre. Kérdést nem tölt és játékállapotot
 * nem tárol.
 */

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
    private readonly SemaphoreSlim _gate = new(1, 1);
    private readonly Dictionary<int, List<VsRankedQueueEntry>> _queues =
        VsBattleClassificationRules.List.ToDictionary(
            rule => rule.ClassificationId,
            _ => new List<VsRankedQueueEntry>());
    private readonly HashSet<int> _lockingPlayers = [];

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

    public async Task JoinAsync(
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
            await RejectAsync(
                connectionId,
                "vsgame.Match.Error.Classification");
            return;
        }

        if (_matchStore.ContainsPlayer(playerId))
        {
            await RejectAsync(
                connectionId,
                "vsgame.Match.Error.AlreadyLocked");
            return;
        }

        var entry = await BuildQueueEntryAsync(
            playerId,
            sessionId,
            connectionId,
            rule,
            ct);

        if (entry is null)
        {
            await RejectAsync(
                connectionId,
                "vsgame.Match.Error.QueueValidation");
            return;
        }

        List<VsRankedQueueEntry>? matchedEntries = null;
        var rejectedAfterValidation = false;

        await _gate.WaitAsync(ct);
        try
        {
            if (_lockingPlayers.Contains(playerId) ||
                _matchStore.ContainsPlayer(playerId))
            {
                rejectedAfterValidation = true;
            }
            else
            {
                RemoveExistingEntry(playerId, connectionId);
                _queues[classificationId].Add(entry);

                if (_queues[classificationId].Count >=
                    VsMatchProfiles.Ranked.RequiredPlayers)
                {
                    matchedEntries =
                    [
                        .. _queues[classificationId]
                            .Take(VsMatchProfiles.Ranked.RequiredPlayers)
                    ];

                    _queues[classificationId].RemoveRange(
                        0,
                        matchedEntries.Count);

                    foreach (var matched in matchedEntries)
                        _lockingPlayers.Add(matched.PlayerId);
                }
            }
        }
        finally
        {
            _gate.Release();
        }

        if (rejectedAfterValidation)
        {
            await RejectAsync(
                connectionId,
                "vsgame.Match.Error.AlreadyLocked");
            return;
        }

        await BroadcastQueueAsync(classificationId);

        if (matchedEntries is not null)
        {
            try
            {
                await _matchService.CreateLockedMatchAsync(
                    matchedEntries,
                    CancellationToken.None);
            }
            finally
            {
                await _gate.WaitAsync();
                try
                {
                    foreach (var matched in matchedEntries)
                        _lockingPlayers.Remove(matched.PlayerId);
                }
                finally
                {
                    _gate.Release();
                }
            }
        }
    }

    public async Task LeaveAsync(
        string connectionId,
        CancellationToken ct = default)
    {
        int? changedClassification = null;

        await _gate.WaitAsync(ct);
        try
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
        finally
        {
            _gate.Release();
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
                category is <
                    VsLoadoutCategoryIds.MinimumFactoryCategory or >
                    VsLoadoutCategoryIds.AllCategories))
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
        VsRankedQueueEntry[] entries;

        await _gate.WaitAsync();
        try
        {
            entries = [.. _queues[classificationId]];
        }
        finally
        {
            _gate.Release();
        }

        var rule = VsBattleClassificationRules.List.First(
            item => item.ClassificationId == classificationId);

        foreach (var currentEntry in entries)
        {
            var snapshot = new VsRankedQueueSnapshot
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
            };

            await _hub.Clients
                .Client(currentEntry.ConnectionId)
                .QueueChanged(snapshot);
        }
    }

    private Task RejectAsync(
        string connectionId,
        string messageKey) =>
        _hub.Clients
            .Client(connectionId)
            .CommandRejected(messageKey);
}

/**
 * MÓDOSÍTÁS: minden várakozó személyre szabott publikus roster-
 * snapshotot kap, ezért a lobby bal oldali játékoslistája is
 * kirajzolható.
 *
 * Az öt besorolás külön várólistáját kezeli, cache-snapshotból
 * validálja a belépést, majd a profil szerinti játékosszámnál
 * MatchLocked meccset hoz létre. Kérdést nem tölt és játékállapotot
 * nem tárol.
 */

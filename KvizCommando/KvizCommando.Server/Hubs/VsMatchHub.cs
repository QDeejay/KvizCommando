using KvizCommando.Server.Authorization;
using KvizCommando.Server.Services.UserPlayerIdCache;
using KvizCommando.Server.Services.VsGame.Match;
using KvizCommando.Server.Services.VsGame.Matchmaking;
using KvizCommando.Shared.Contracts.VsGame.Match;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace KvizCommando.Server.Hubs;

[Authorize(Policy = TermsAcceptedRequirement.PolicyName)]
public sealed class VsMatchHub : Hub<IVsMatchHubClient>
{
    private readonly IUserPlayerIdCacheService _idCache;
    private readonly IVsRankedQueueService _queue;
    private readonly IVsMatchService _matches;

    public VsMatchHub(
        IUserPlayerIdCacheService idCache,
        IVsRankedQueueService queue,
        IVsMatchService matches)
    {
        _idCache = idCache;
        _queue = queue;
        _matches = matches;
    }

    public async Task<VsQueueJoinResult> JoinRankedQueue(
        int classificationId,
        string sessionId)
    {
        var ct = Context.ConnectionAborted;
        var playerId = await ResolvePlayerIdAsync(ct);

        return await _queue.JoinAsync(
            playerId,
            sessionId,
            Context.ConnectionId,
            classificationId,
            ct);
    }

    public Task LeaveRankedQueue() =>
        _queue.LeaveAsync(
            Context.ConnectionId,
            Context.ConnectionAborted);

    public Task SelectCharacter(
        int slotNumber) =>
        _matches.SelectCharacterAsync(
            Context.ConnectionId,
            slotNumber,
            Context.ConnectionAborted);

    public Task AssignLoadout(
        VsLoadoutAssignmentRequest request) =>
        _matches.AssignLoadoutAsync(
            Context.ConnectionId,
            request,
            Context.ConnectionAborted);

    public Task AssignHelp(
        VsHelpAssignmentRequest request) =>
        _matches.AssignHelpAsync(
            Context.ConnectionId,
            request,
            Context.ConnectionAborted);

    public Task ResetPreparation() =>
        _matches.ResetPreparationAsync(
            Context.ConnectionId,
            Context.ConnectionAborted);

    public Task FinishPreparation() =>
        _matches.FinishPreparationAsync(
            Context.ConnectionId,
            Context.ConnectionAborted);

    public Task SubmitGuess(
        VsGuessAnswerRequest request) =>
        _matches.SubmitGuessAsync(
            Context.ConnectionId,
            request,
            Context.ConnectionAborted);

    public Task SubmitChoice(
        VsChoiceAnswerRequest request) =>
        _matches.SubmitChoiceAsync(
            Context.ConnectionId,
            request,
            Context.ConnectionAborted);

    public Task SelectCaptainQuestion(
        VsCaptainQuestionRequest request) =>
        _matches.SelectCaptainQuestionAsync(
            Context.ConnectionId,
            request,
            Context.ConnectionAborted);

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await _queue.LeaveAsync(
            Context.ConnectionId,
            CancellationToken.None);

        await _matches.DisconnectAsync(
            Context.ConnectionId,
            CancellationToken.None);

        await base.OnDisconnectedAsync(exception);
    }

    private async Task<int> ResolvePlayerIdAsync(CancellationToken ct)
    {
        var userId =
            Context.User?.FindFirstValue(ClaimTypes.NameIdentifier) ??
            Context.User?.FindFirstValue("sub") ??
            throw new HubException("vsgame.Match.Error.Identity");

        var playerId = await _idCache.GetPlayerIdAsync(userId, ct);

        return playerId is > 0
            ? playerId.Value
            : throw new HubException("vsgame.Match.Error.Player");
    }
}

/**
 * MÓDOSÍTÁS: a queue-belépés várható eredményét közvetlenül visszaadja
 * a hívó kliensnek. A Hub továbbra sem tart fenn játékállapotot.
 *
 * MÓDOSÍTÁS: a tipp-, válasz- és kapitánykérdés-parancsot ugyanúgy,
 * állapot nélkül továbbítja a match service felé.
 *
 * A VS kliens parancsait azonosítja és továbbítja a queue- vagy
 * match-szerviznek. Saját játékállapotot és adatbázislogikát nem
 * tartalmaz.
 */

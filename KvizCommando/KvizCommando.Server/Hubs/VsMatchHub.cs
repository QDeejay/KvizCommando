using KvizCommando.Server.Authorization;
using KvizCommando.Server.Services.UserPlayerIdCache;
using KvizCommando.Server.Services.VsGame.Match;
using KvizCommando.Server.Services.VsGame.Matchmaking;
using KvizCommando.Shared.Contracts.VsGame.Match;
using KvizCommando.Shared.Models.Enums.VsGame;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Diagnostics;
using System.Security.Claims;

namespace KvizCommando.Server.Hubs;

[Authorize(Policy = TermsAcceptedRequirement.PolicyName)]
public sealed class VsMatchHub : Hub<IVsMatchHubClient>
{
    private const int RESPONSE_TIME_PROBE_COUNT = 5;
    private const int RESPONSE_TIME_SAMPLES = 4;

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

        if (!Context.Items.TryGetValue(
                typeof(ConnectionCheckState),
                out var value) ||
            value is not ConnectionCheckState connectionCheck)
        {
            return new VsQueueJoinResult
            {
                ErrorKey = "vsgame.Match.Error.Connection"
            };
        }

        if (connectionCheck.Quality == VsConnectionQuality.Bad)
        {
            return new VsQueueJoinResult
            {
                ErrorKey =
                    "vsgame.Match.Error.ConnectionSpeed"
            };
        }

        var playerId = await ResolvePlayerIdAsync(ct);

        return await _queue.JoinAsync(
            playerId,
            sessionId,
            Context.ConnectionId,
            classificationId,
            connectionCheck.ResponseTimeMilliseconds,
            connectionCheck.Quality,
            ct);
    }

    public async Task<VsConnectionCheckResult> CheckConnection()
    {
        var responseTimeMilliseconds =
            await MeasureResponseTimeAsync(
                Context.ConnectionAborted);

        var quality = ResolveConnectionQuality(
            responseTimeMilliseconds);

        Context.Items[typeof(ConnectionCheckState)] =
            new ConnectionCheckState(
                responseTimeMilliseconds,
                quality);

        return new VsConnectionCheckResult
        {
            ResponseTimeMilliseconds =
                responseTimeMilliseconds,
            Quality = quality
        };
    }

    public DateTime GetServerUtc() => DateTime.UtcNow;

    public Task<VsQueueLeaveStatus> LeaveRankedQueue() =>
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

    public Task UseHelp(
        VsUseHelpRequest request) =>
        _matches.UseHelpAsync(
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
        await _queue.DisconnectAsync(
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

    private async Task<int> MeasureResponseTimeAsync(
        CancellationToken ct)
    {
        double totalMilliseconds = 0;

        for (var index = 0;
             index < RESPONSE_TIME_PROBE_COUNT;
             index++)
        {
            ct.ThrowIfCancellationRequested();

            var token = Random.Shared.NextInt64();
            var stopwatch = Stopwatch.StartNew();
            var response = await Clients.Caller
                .LatencyProbe(token)
                .WaitAsync(ct);
            stopwatch.Stop();

            if (response != token)
                throw new HubException(
                    "vsgame.Match.Error.Connection");

            if (index > 0)
                totalMilliseconds +=
                    stopwatch.Elapsed.TotalMilliseconds;
        }

        return (int)Math.Round(
            totalMilliseconds / RESPONSE_TIME_SAMPLES,
            MidpointRounding.AwayFromZero);
    }

    private static VsConnectionQuality ResolveConnectionQuality(
        int responseTimeMilliseconds)
    {
        var profile = VsMatchProfiles.Ranked;

        if (responseTimeMilliseconds <=
            profile.GoodResponseTimeMilliseconds)
        {
            return VsConnectionQuality.Good;
        }

        return responseTimeMilliseconds <=
               profile.MaximumResponseTimeMilliseconds
            ? VsConnectionQuality.Medium
            : VsConnectionQuality.Bad;
    }

    private sealed record ConnectionCheckState(
        int ResponseTimeMilliseconds,
        VsConnectionQuality Quality);
}

/**
 * MÓDOSÍTÁS: a queue-belépés várható eredményét közvetlenül visszaadja
 * a hívó kliensnek. A Hub továbbra sem tart fenn játékállapotot.
 *
 * MÓDOSÍTÁS: a tipp-, válasz-, segítség- és kapitánykérdés-parancsot
 * ugyanúgy, állapot nélkül továbbítja a match service felé.
 * MÓDOSÍTÁS: a kliens egyszeri óraszinkronjához visszaadja az aktuális
 * szerver UTC-időt; folyamatos időüzenetet nem tart fenn.
 *
 * A VS kliens parancsait azonosítja és továbbítja a queue- vagy
 * match-szerviznek. Saját játékállapotot és adatbázislogikát nem
 * tartalmaz.
 * MÓDOSÍTÁS: queue-belépés előtt öt SignalR-visszhangot mér, az elsőt
 * eldobja, a következő négy átlagát pedig a kapcsolat Context.Items
 * állapotában tartja. Rossz minősítésű vagy mérés nélküli kapcsolat
 * nem léphet be a várólistába.
 * MÓDOSÍTÁS: a kliens explicit LeaveRankedQueue parancsa visszajelzi,
 * hogy valóban történt-e manuális kilépés és jár-e cooldown. A
 * SignalR-disconnect külön büntetésmentes eltávolítás marad.
 */

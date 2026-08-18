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

[Authorize(Policy = TermsAcceptedRequirement.POLICY_NAME)]
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

    /// <summary>
    /// Belépteti a kapcsolat játékosát a rangsorolt várólistába.
    /// </summary>
    /// <param name="classificationId">A kiválasztott rangsorolt várólista azonosítója.</param>
    /// <param name="sessionId">A kliens aktuális munkamenet-azonosítója.</param>
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

    /// <summary>
    /// Megméri a kliens és a szerver közötti SignalR-kapcsolat válaszidejét.
    /// </summary>
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

    /// <summary>
    /// Visszaadja a szerver aktuális UTC-időpontját.
    /// </summary>
    public DateTime GetServerUtc() => DateTime.UtcNow;

    /// <summary>
    /// Kilépteti a kapcsolat játékosát a rangsorolt várólistából.
    /// </summary>
    public Task<VsQueueLeaveStatus> LeaveRankedQueue() =>
        _queue.LeaveAsync(
            Context.ConnectionId,
            Context.ConnectionAborted);

    /// <summary>
    /// A karaktert a megadott előkészítési helyhez rendeli.
    /// </summary>
    /// <param name="slotNumber">Az előkészítési hely egytől induló sorszáma.</param>
    public Task SelectCharacter(
        int slotNumber) =>
        _matches.SelectCharacterAsync(
            Context.ConnectionId,
            slotNumber,
            Context.ConnectionAborted);

    /// <summary>
    /// A kiválasztott kérdéskategóriát a megadott körhöz rendeli.
    /// </summary>
    /// <param name="request">A feldolgozandó kérés adatai.</param>
    public Task AssignLoadout(
        VsLoadoutAssignmentRequest request) =>
        _matches.AssignLoadoutAsync(
            Context.ConnectionId,
            request,
            Context.ConnectionAborted);

    /// <summary>
    /// A kiválasztott segítséget a megadott előkészítési helyhez rendeli.
    /// </summary>
    /// <param name="request">A feldolgozandó kérés adatai.</param>
    public Task AssignHelp(
        VsHelpAssignmentRequest request) =>
        _matches.AssignHelpAsync(
            Context.ConnectionId,
            request,
            Context.ConnectionAborted);

    /// <summary>
    /// Törli a játékos előkészítési választásait.
    /// </summary>
    public Task ResetPreparation() =>
        _matches.ResetPreparationAsync(
            Context.ConnectionId,
            Context.ConnectionAborted);

    /// <summary>
    /// Lezárja a játékos előkészítési szakaszát.
    /// </summary>
    public Task FinishPreparation() =>
        _matches.FinishPreparationAsync(
            Context.ConnectionId,
            Context.ConnectionAborted);

    /// <summary>
    /// Beküldi a becslős meccskérdés válaszát.
    /// </summary>
    /// <param name="request">A feldolgozandó kérés adatai.</param>
    public Task SubmitGuess(
        VsGuessAnswerRequest request) =>
        _matches.SubmitGuessAsync(
            Context.ConnectionId,
            request,
            Context.ConnectionAborted);

    /// <summary>
    /// Beküldi a feleletválasztós meccskérdés válaszát.
    /// </summary>
    /// <param name="request">A feldolgozandó kérés adatai.</param>
    public Task SubmitChoice(
        VsChoiceAnswerRequest request) =>
        _matches.SubmitChoiceAsync(
            Context.ConnectionId,
            request,
            Context.ConnectionAborted);

    /// <summary>
    /// Felhasználja a kiválasztott segítséget az aktuális kérdésnél.
    /// </summary>
    /// <param name="request">A feldolgozandó kérés adatai.</param>
    public Task UseHelp(
        VsUseHelpRequest request) =>
        _matches.UseHelpAsync(
            Context.ConnectionId,
            request,
            Context.ConnectionAborted);

    /// <summary>
    /// Kiválasztja a kapitányi kör kérdését.
    /// </summary>
    /// <param name="request">A feldolgozandó kérés adatai.</param>
    public Task SelectCaptainQuestion(
        VsCaptainQuestionRequest request) =>
        _matches.SelectCaptainQuestionAsync(
            Context.ConnectionId,
            request,
            Context.ConnectionAborted);

    /// <summary>
    /// Feldolgozza a SignalR-kapcsolat megszakadását.
    /// </summary>
    /// <param name="exception">A kapcsolat megszakadását kiváltó kivétel, ha ismert.</param>
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

using KvizCommando.Server.Authorization;
using KvizCommando.Server.Services.SoloGame;
using KvizCommando.Server.Services.UserPlayerIdCache;
using KvizCommando.Server.Services.VsGame.Match;
using KvizCommando.Shared.Contracts.SoloGame;
using KvizCommando.Shared.Contracts.VsGame.Match;
using KvizCommando.Shared.Models.Enums.VsGame;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Diagnostics;
using System.Security.Claims;

namespace KvizCommando.Server.Hubs;

[Authorize(Policy = TermsAcceptedRequirement.PolicyName)]
public sealed class SoloGameHub : Hub<ISoloGameHubClient>
{
    private const int RESPONSE_TIME_PROBE_COUNT = 5;
    private const int RESPONSE_TIME_SAMPLES = 4;

    private readonly IUserPlayerIdCacheService _idCache;
    private readonly ISoloGameService _games;

    public SoloGameHub(
        IUserPlayerIdCacheService idCache,
        ISoloGameService games)
    {
        _idCache = idCache;
        _games = games;
    }

    public async Task<VsConnectionCheckResult> CheckConnection()
    {
        var responseTimeMilliseconds =
            await MeasureResponseTimeAsync(
                RESPONSE_TIME_PROBE_COUNT,
                RESPONSE_TIME_SAMPLES,
                Context.ConnectionAborted);
        var result = BuildConnectionResult(
            responseTimeMilliseconds);

        Context.Items[typeof(VsConnectionCheckResult)] = result;
        return result;
    }

    public async Task<StartSoloHubResponse> StartSoloGame(
        StartSoloGameRequest request)
    {
        if (!Context.Items.TryGetValue(
                typeof(VsConnectionCheckResult),
                out var checkValue) ||
            checkValue is not VsConnectionCheckResult check)
        {
            return RejectStart("solo.Error.Connection");
        }

        if (check.Quality == VsConnectionQuality.Bad)
            return RejectStart("solo.Error.ConnectionSpeed");

        if (string.IsNullOrWhiteSpace(request.SessionId) ||
            request.SelectionId < 1)
        {
            return RejectStart("solo.Error.InvalidData");
        }

        var playerId = await ResolvePlayerIdAsync(
            Context.ConnectionAborted);
        var result = await _games.StartSignalRAsync(
            playerId,
            request,
            Context.ConnectionAborted);

        if (result.Success != true || result.Response is null)
        {
            return RejectStart(
                result.Success is null
                    ? "solo.Error.Session"
                    : "solo.Error.ActiveGame");
        }

        Context.Items[typeof(SoloHubGameState)] =
            new SoloHubGameState(
                playerId,
                result.Response.GameId,
                request.SessionId);

        return new StartSoloHubResponse
        {
            IsAccepted = true,
            Game = result.Response
        };
    }

    public async Task<SoloHubAnswerResponse> SubmitAnswer(
        SoloAnswerDto answer)
    {
        if (!TryGetGame(out var game))
            return RejectAnswer("solo.Error.InvalidAnswer");

        var result = await _games.SubmitAnswerAsync(
            game.PlayerId,
            game.GameId,
            answer,
            Context.ConnectionAborted);

        if (result.Success != true)
            return RejectAnswer("solo.Error.InvalidAnswer");

        var responseTimeMilliseconds =
            await MeasureResponseTimeAsync(
                1,
                1,
                Context.ConnectionAborted);

        if (result.Response is not null)
            Context.Items.Remove(typeof(SoloHubGameState));

        return new SoloHubAnswerResponse
        {
            IsAccepted = true,
            Connection = BuildConnectionResult(
                responseTimeMilliseconds),
            Result = result.Response
        };
    }

    public async Task<bool> AbortSoloGame()
    {
        if (!TryGetGame(out var game))
            return false;

        var result = await _games.AbandonAsync(
            game.PlayerId,
            game.GameId,
            game.SessionId,
            Context.ConnectionAborted);

        Context.Items.Remove(typeof(SoloHubGameState));
        return result == true;
    }

    private bool TryGetGame(out SoloHubGameState game)
    {
        if (Context.Items.TryGetValue(
                typeof(SoloHubGameState),
                out var value) &&
            value is SoloHubGameState state)
        {
            game = state;
            return true;
        }

        game = default!;
        return false;
    }

    private async Task<int> ResolvePlayerIdAsync(
        CancellationToken ct)
    {
        var userId =
            Context.User?.FindFirstValue(
                ClaimTypes.NameIdentifier) ??
            Context.User?.FindFirstValue("sub") ??
            throw new HubException("solo.Error.Identity");

        var playerId = await _idCache.GetPlayerIdAsync(userId, ct);

        return playerId is > 0
            ? playerId.Value
            : throw new HubException("solo.Error.Player");
    }

    private async Task<int> MeasureResponseTimeAsync(
        int probeCount,
        int sampleCount,
        CancellationToken ct)
    {
        double totalMilliseconds = 0;

        for (var index = 0; index < probeCount; index++)
        {
            var token = Random.Shared.NextInt64();
            var stopwatch = Stopwatch.StartNew();
            var response = await Clients.Caller
                .LatencyProbe(token)
                .WaitAsync(ct);
            stopwatch.Stop();

            if (response != token)
                throw new HubException("solo.Error.Connection");

            if (probeCount == 1 || index > 0)
                totalMilliseconds +=
                    stopwatch.Elapsed.TotalMilliseconds;
        }

        return (int)Math.Round(
            totalMilliseconds / sampleCount,
            MidpointRounding.AwayFromZero);
    }

    private static VsConnectionCheckResult BuildConnectionResult(
        int responseTimeMilliseconds) =>
        new()
        {
            ResponseTimeMilliseconds = responseTimeMilliseconds,
            Quality = ResolveConnectionQuality(
                responseTimeMilliseconds)
        };

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

    private static StartSoloHubResponse RejectStart(
        string errorKey) =>
        new()
        {
            ErrorKey = errorKey
        };

    private static SoloHubAnswerResponse RejectAnswer(
        string errorKey) =>
        new()
        {
            ErrorKey = errorKey
        };

    private sealed record SoloHubGameState(
        int PlayerId,
        Guid GameId,
        string SessionId);
}

/**
 * ÚJ FÁJL: a kategória- és orientációs Solo mód közös, állapotmentes
 * SignalR belépési pontja. Start előtt ugyanazt az ötpróbás mérést
 * végzi, mint a VS, válaszonként egy friss próbát ad vissza. A hub
 * csak az adott kapcsolat game-, player- és sessionazonosítóját őrzi;
 * a játékállapot a meglévő SoloGameSession cache-ben marad.
 * Disconnectkor szándékosan nem abandonol: a session lejárata takarít.
 */

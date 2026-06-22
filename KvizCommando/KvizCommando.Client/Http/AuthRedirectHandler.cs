using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using KvizCommando.Client.Services.Visual;
using Microsoft.AspNetCore.Components;

public class AuthRedirectHandler : DelegatingHandler
{
    private readonly NavigationManager _navigation;
    private readonly LoaderService _loader;

    public AuthRedirectHandler(NavigationManager navigation, LoaderService loader)
    {
        _navigation = navigation;
        _loader = loader;
    }
    private static readonly string[] _ignore401Endpoints =
        {
            "/start",
            "/login",
            "/register",
            "/checkin",
            "/refresh"
        };

    protected override async Task<HttpResponseMessage> SendAsync(
     HttpRequestMessage request,
     CancellationToken cancellationToken)
    {
        _loader.Trigger();
        var response = await base.SendAsync(request, cancellationToken);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            // nézd meg, hogy a kérés path-ja benne van-e az ignore listában
            var path = request.RequestUri.AbsolutePath.ToLowerInvariant();

            if (!_ignore401Endpoints.Any(ignore => path.Contains(ignore)))
            {
                _navigation.NavigateTo("/login", true);
            }
        }

        return response;
    }
}

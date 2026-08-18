using Microsoft.AspNetCore.Components;
using System.Net;

public class AuthRedirectHandler : DelegatingHandler
{
    private readonly NavigationManager _navigation;

    public AuthRedirectHandler(NavigationManager navigation)
    {
        _navigation = navigation;
    }
    private static readonly HashSet<string> _ignore401Endpoints =
[
    "/",
    "/?reason=expired",
    "/login",
    "/register",
    "/api/checkin",
    "/refresh"
];
    protected override async Task<HttpResponseMessage> SendAsync(
     HttpRequestMessage request,
     CancellationToken cancellationToken)
    {
        var response = await base.SendAsync(request, cancellationToken);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            var path = request.RequestUri!.AbsolutePath.ToLowerInvariant();

            if (path == "/api/logout")
                return response;

            if (!_ignore401Endpoints.Contains(path))
            {
                _navigation.NavigateTo("/?reason=expired", forceLoad: true);
            }
        }
        return response;
    }
}

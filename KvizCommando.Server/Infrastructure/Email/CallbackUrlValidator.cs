using System.Diagnostics.CodeAnalysis;

namespace KvizCommando.Server.Infrastructure.Email;

public class CallbackUrlValidator : ICallbackUrlValidator
{
    private readonly HashSet<string> _allowedHosts;

    public CallbackUrlValidator(Microsoft.Extensions.Options.IOptions<CallbackWhitelistOptions> options)
    {
        _allowedHosts = options.Value.AllowedDomains
            .Where(d => !string.IsNullOrWhiteSpace(d))
            .Select(NormalizeHost)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Jelzi, hogy az abszolút URL hostja és sémája engedélyezett-e.
    /// </summary>
    public bool IsAllowedAbsoluteUrl(string? absoluteUrl)
    {
        if (string.IsNullOrWhiteSpace(absoluteUrl)) return false;
        if (!Uri.TryCreate(absoluteUrl, UriKind.Absolute, out var uri)) return false;
        return _allowedHosts.Contains(NormalizeHost(uri.Host));
    }

    /// <summary>
    /// Engedélyezett abszolút callback URL-t állít elő a megadott visszatérési címből.
    /// </summary>
    public Uri? TryBuildWhitelistedAbsoluteUrl(string? returnUrl, Uri serverBaseUri)
    {
        if (string.IsNullOrWhiteSpace(returnUrl)) return null;

        // Ha abszolút URL
        if (Uri.TryCreate(returnUrl, UriKind.Absolute, out var absolute))
        {
            return _allowedHosts.Contains(NormalizeHost(absolute.Host)) ? absolute : null;
        }

        // Ha relatív, a serverBaseUri-t használjuk alapnak, de a hostot whitelisteljük
        if (Uri.TryCreate(serverBaseUri, returnUrl, out var combined))
        {
            return _allowedHosts.Contains(NormalizeHost(combined.Host)) ? combined : null;
        }

        return null;
    }

    [return: NotNullIfNotNull(nameof(host))]
    private static string? NormalizeHost(string? host)
        => host?.Trim().TrimEnd('.').ToLowerInvariant();
}

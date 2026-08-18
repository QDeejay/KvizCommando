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

    /// <inheritdoc />
    public bool IsAllowedAbsoluteUrl(string? absoluteUrl)
    {
        if (string.IsNullOrWhiteSpace(absoluteUrl)) return false;
        if (!Uri.TryCreate(absoluteUrl, UriKind.Absolute, out var uri)) return false;
        return _allowedHosts.Contains(NormalizeHost(uri.Host));
    }

    /// <inheritdoc />
    public Uri? TryBuildWhitelistedAbsoluteUrl(string? returnUrl, Uri serverBaseUri)
    {
        if (string.IsNullOrWhiteSpace(returnUrl)) return null;

        if (Uri.TryCreate(returnUrl, UriKind.Absolute, out var absolute))
        {
            return _allowedHosts.Contains(NormalizeHost(absolute.Host)) ? absolute : null;
        }

        // Relatív címnél a feloldás után is ellenőrizni kell a hostot, különben nyílt átirányítás kerülhet a folyamatba.
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

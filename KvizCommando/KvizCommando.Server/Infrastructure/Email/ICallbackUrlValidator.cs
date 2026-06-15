namespace KvizCommando.Server.Infrastructure.Email;

public interface ICallbackUrlValidator
{
    /// <summary>
    /// Igazat ad vissza, ha a megadott abszolút URL hostja az engedélyezett domain(ek) egyike.
    /// </summary>
    bool IsAllowedAbsoluteUrl(string? absoluteUrl);

    /// <summary>
    /// A megadott returnUrl (lehet relatív vagy abszolút) alapján kanonikus, whitelistelt abszolút URL-t ad vissza,
    /// vagy null-t, ha nem engedélyezett.
    /// </summary>
    Uri? TryBuildWhitelistedAbsoluteUrl(string? returnUrl, Uri serverBaseUri);
}

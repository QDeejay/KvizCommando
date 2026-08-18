namespace KvizCommando.Server.Infrastructure.Email;

public interface ICallbackUrlValidator
{
    /// <summary>
    /// Ellenőrzi, hogy az abszolút URL állomásneve szerepel-e az engedélyezett tartományok között.
    /// </summary>
    /// <param name="absoluteUrl">Az ellenőrzendő abszolút URL; <see langword="null"/> esetén az ellenőrzés sikertelen.</param>
    /// <returns><see langword="true"/>, ha a cím abszolút és szerepel az engedélyezett célok között.</returns>
    bool IsAllowedAbsoluteUrl(string? absoluteUrl);

    /// <summary>
    /// Relatív vagy abszolút visszatérési címből ellenőrzött, kanonikus abszolút URI-t képez.
    /// </summary>
    /// <param name="returnUrl">Az ellenőrzendő relatív vagy abszolút visszahívási cím.</param>
    /// <param name="serverBaseUri">A relatív visszahívási cím feloldásához használt szerver-alapcím.</param>
    /// <returns>Az engedélyezett abszolút URI, vagy <see langword="null"/>, ha a cím hibás vagy nem engedélyezett.</returns>
    Uri? TryBuildWhitelistedAbsoluteUrl(string? returnUrl, Uri serverBaseUri);
}

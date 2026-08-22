namespace KvizCommando.Server.Application.Abstractions.Security;

public interface IRegistrationBenefitClaimService
{
    /// <summary>Visszaadja, hogy az e-mail-cím mikortól jogosult ismét regisztrációs kedvezményre.</summary>
    /// <param name="email">A vizsgált e-mail-cím.</param>
    /// <param name="ct">A művelet megszakítását jelző token.</param>
    /// <returns>A következő jogosultság UTC időpontja, vagy <see langword="null"/>, ha nincs tárolt rekord.</returns>
    Task<DateTime?> GetEligibleAgainAtUtcAsync(
        string email,
        CancellationToken ct = default);

    /// <summary>Elmenti vagy meghosszabbítja a normalizált e-mail vissza nem fejthető jogosultsági ujjlenyomatát.</summary>
    /// <param name="email">A korábban használt e-mail-cím.</param>
    /// <param name="eligibleAgainAtUtc">Az új regisztrációs kedvezmény legkorábbi UTC időpontja.</param>
    /// <param name="ct">A művelet megszakítását jelző token.</param>
    Task RecordAsync(string email, DateTime eligibleAgainAtUtc, CancellationToken ct = default);
}

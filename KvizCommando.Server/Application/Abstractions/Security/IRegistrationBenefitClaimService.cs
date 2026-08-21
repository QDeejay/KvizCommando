namespace KvizCommando.Server.Application.Abstractions.Security;

public interface IRegistrationBenefitClaimService
{
    /// <summary>Elmenti vagy meghosszabbítja a normalizált e-mail vissza nem fejthető jogosultsági ujjlenyomatát.</summary>
    /// <param name="email">A korábban használt e-mail-cím.</param>
    /// <param name="eligibleAgainAtUtc">Az új regisztrációs kedvezmény legkorábbi UTC időpontja.</param>
    /// <param name="ct">A művelet megszakítását jelző token.</param>
    Task RecordAsync(string email, DateTime eligibleAgainAtUtc, CancellationToken ct = default);
}

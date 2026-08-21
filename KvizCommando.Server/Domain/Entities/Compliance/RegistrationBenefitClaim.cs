namespace KvizCommando.Server.Domain.Entities.Compliance;

public sealed class RegistrationBenefitClaim
{
    public string EmailFingerprint { get; set; } = null!;
    public DateTime EligibleAgainAtUtc { get; set; }
}

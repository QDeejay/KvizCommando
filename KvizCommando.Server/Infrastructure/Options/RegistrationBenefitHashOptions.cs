namespace KvizCommando.Server.Infrastructure.Options;

public sealed class RegistrationBenefitHashOptions
{
    public const string SECTION_NAME = "RegistrationBenefitHash";
    public string Secret { get; set; } = string.Empty;
}

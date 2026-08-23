namespace KvizCommando.Server.Application.Security;

/// <summary>
/// A regisztrációs kedvezmény ÁSZF-ben rögzített üzleti szabályai.
/// Ezen értékek megváltoztatása új ÁSZF-verzió kiadását igényli.
/// </summary>
public static class RegistrationBenefitRules
{
    public const int STARTING_CREDIT = 1000;
    public const int BENEFIT_BLOCK_DAYS = 30;
}

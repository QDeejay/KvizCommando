namespace KvizCommando.Shared.Contracts.Auth
{
    public class RegisterOptionsResponse
    {
        // Jelszóházirend
        public int RequiredLength { get; set; }
        public bool RequireNonAlphanumeric { get; set; }
        public bool RequireDigit { get; set; }
        public bool RequireLowercase { get; set; }
        public bool RequireUppercase { get; set; }
        public int RequiredUniqueChars { get; set; }

        // Felhasználói fiók
        public bool RequireUniqueEmail { get; set; }

        // Bejelentkezési követelmények
        public bool RequireConfirmedEmail { get; set; }
        public bool RequireConfirmedAccount { get; set; }
        public bool RegistrationEnabled { get; set; } = true;
        public bool FacebookLoginEnabled { get; set; }
        public string InvitationTestPeriod { get; set; } = string.Empty;
        public string SupportEmail { get; set; } = string.Empty;

        // Alkalmazásspecifikus szabályok
        public int DisplayNameMaxLength { get; set; }
        public int DisplayNameMinLength { get; set; }

        // Kizárólag Development környezetben, fájlos kézbesítésnél kap értéket.
        public string? RegistrationEmailOutputPath { get; set; }
        public string? PasswordResetEmailOutputPath { get; set; }
    }
}

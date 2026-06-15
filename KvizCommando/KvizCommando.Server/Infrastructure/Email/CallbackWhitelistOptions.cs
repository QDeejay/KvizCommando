namespace KvizCommando.Server.Infrastructure.Email;

public class CallbackWhitelistOptions
{
    public string[] AllowedDomains { get; set; } = Array.Empty<string>();
}

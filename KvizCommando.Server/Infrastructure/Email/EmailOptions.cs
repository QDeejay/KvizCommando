namespace KvizCommando.Server.Infrastructure.Email;

public sealed class EmailOptions
{
    public const string SECTION_NAME = "Email";
    public const string FILE_SERVICE = "File";
    public const string MAIL_SERVICE = "Mail";

    public string Service { get; set; } = FILE_SERVICE;
    public string OutputRoot { get; set; } = "App/Email";
    public string ActiveBaseUrl { get; set; } = "PublicTunnel";
    public Dictionary<string, string> BaseUrls { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Feloldja az aktív e-mail-hivatkozások konfigurált alapcímét.</summary>
    /// <returns>Az abszolút alapcím.</returns>
    /// <exception cref="InvalidOperationException">Az aktív bejegyzés hiányzik vagy nem érvényes abszolút URI.</exception>
    public Uri GetActiveBaseUri()
    {
        if (!BaseUrls.TryGetValue(ActiveBaseUrl, out var value) ||
            !Uri.TryCreate(value, UriKind.Absolute, out var uri))
        {
            throw new InvalidOperationException(
                $"Az Email:ActiveBaseUrl beállítás nem érvényes: '{ActiveBaseUrl}'.");
        }

        return uri;
    }
}

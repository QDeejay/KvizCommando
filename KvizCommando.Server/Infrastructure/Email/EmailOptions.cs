namespace KvizCommando.Server.Infrastructure.Email;

public sealed class EmailOptions
{
    public const string SectionName = "Email";
    public const string FileService = "File";
    public const string MailService = "Mail";

    public string Service { get; set; } = FileService;
    public string OutputRoot { get; set; } = @"C:\KvizCommando\Email";
    public string ActiveBaseUrl { get; set; } = "PublicTunnel";
    public Dictionary<string, string> BaseUrls { get; set; } = new(StringComparer.OrdinalIgnoreCase);

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

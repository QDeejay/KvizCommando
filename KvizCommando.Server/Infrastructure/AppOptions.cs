namespace KvizCommando.Server.Infrastructure.Options
{
    public class AppOptions
    {
        public string Name { get; set; } = string.Empty;
        public string SupportEmail { get; set; } = string.Empty;

        public string TempServerIp { get; set; } = string.Empty;

        public string WebUrl { get; set; } = string.Empty;
        public int TokenValidityHours { get; set; } = 24;


    }
}

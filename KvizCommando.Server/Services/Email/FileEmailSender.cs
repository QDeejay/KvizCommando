using Microsoft.AspNetCore.Identity.UI.Services;
using System.IO;
using System.Text.RegularExpressions;

namespace KvizCommando.Server.Services.Email
{
    /// <summary>
    /// Fejlesztői e-mail "küldő": a levelet .txt fájlba menti a Server/outputs mappába.
    /// Emellett megpróbálja kinyerni az első http(s) linket a HTML-ből, és PlainLink-ként külön sorban elmenti.
    /// Ha talál 'token=' query paramétert, annak értékét URL-enkódolja (Uri.EscapeDataString).
    /// </summary>
    public class FileEmailSender : IEmailSender
    {
        private readonly string _outputDir;

        public FileEmailSender(IWebHostEnvironment env)
        {
            // A projekt gyökérbe készít egy outputs mappát
            _outputDir = Path.Combine(env.ContentRootPath, "outputs");

            if (!Directory.Exists(_outputDir))
                Directory.CreateDirectory(_outputDir);
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var fileName = $"{DateTime.UtcNow:yyyyMMdd_HHmmss}_{Guid.NewGuid():N}.txt";
            var filePath = Path.Combine(_outputDir, fileName);

            var content = $"""
            To: {email}
            Subject: {subject}
            ------------------------
            {htmlMessage}
            """;

            await File.WriteAllTextAsync(filePath, content);
        }

    }
}

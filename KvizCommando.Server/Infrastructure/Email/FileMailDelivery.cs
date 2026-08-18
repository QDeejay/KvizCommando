using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace KvizCommando.Infrastructure.Email
{
    /// <summary>
    /// Fejlesztési e-mail-kézbesítő, amely a leveleket helyi fájlokba írja.
    /// Nem alkalmas éles levélküldésre; a csere feltételeit a docs/infrastructure-status.md rögzíti.
    /// </summary>
    public class FileEmailDelivery
    {
        private readonly string _outputDir;

        public FileEmailDelivery()
        {
            _outputDir = _outputDir ?? @"C:\TestEmail";
            Console.WriteLine($"[FileEmailDelivery] Emails will be written to: {_outputDir}");
            Directory.CreateDirectory(_outputDir);
        }

        /// <summary>
        /// Fejlesztési e-mail-fájlt ír a megadott levéltartalommal.
        /// </summary>
        public async Task WriteAsync(string to, string from, string subject, string textBody, string htmlBody, CancellationToken cancellationToken)
        {
            var fileName = $"{DateTime.UtcNow:yyyyMMdd_HHmmssfff}_{Guid.NewGuid():N}.eml";
            var path = Path.Combine(_outputDir, fileName);
           
            var sb = new StringBuilder();
            sb.AppendLine($"From: {from}");
            sb.AppendLine($"To: {to}");
            sb.AppendLine($"Date: {DateTime.UtcNow:R}");
            sb.AppendLine($"Subject: {subject}");
            sb.AppendLine("MIME-Version: 1.0");
            sb.AppendLine("Content-Type: text/plain; charset=utf-8");
            sb.AppendLine();
            sb.AppendLine(htmlBody);  
            sb.AppendLine();
            if (htmlBody == null || htmlBody == "")
            {
                sb.AppendLine("---- Raw ----");
                sb.AppendLine(textBody);
            }
           

            await File.WriteAllTextAsync(path, sb.ToString(), Encoding.UTF8);
        }
    }
}

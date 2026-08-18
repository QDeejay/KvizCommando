using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace KvizCommando.Infrastructure.Email
{
    public class FileEmailDelivery
    {
        private readonly string _outputDir;

        public FileEmailDelivery()
        {
            //_outputDir = Path.Combine(AppContext.BaseDirectory, "TestOutput", "Emails");
            _outputDir = _outputDir ?? @"C:\TestEmail";
            Console.WriteLine($"[FileEmailDelivery] Emails will be written to: {_outputDir}");
            Directory.CreateDirectory(_outputDir);
        }

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

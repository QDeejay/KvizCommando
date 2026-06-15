using BenchmarkDotNet.Attributes;
using System.IO;
using System.Threading.Tasks;
using KvizCommando.Server.Infrastructure.Logging;
using Microsoft.AspNetCore.Hosting;
using Microsoft.VSDiagnostics;

namespace KvizCommando.Server.Benchmarks
{
    [CPUUsageDiagnoser]
    public class AuditLoggerBenchmarks
    {
        private AuditLogger _logger;
        private class FakeEnv : IWebHostEnvironment
        {
            public string EnvironmentName { get; set; } = "Development";
            public string ApplicationName { get; set; } = "KvizCommando.Server";
            public string ContentRootPath { get; set; } = Path.GetTempPath();
            public string WebRootPath { get; set; } = Path.GetTempPath();
            public Microsoft.Extensions.FileProviders.IFileProvider? ContentRootFileProvider { get; set; }
            public Microsoft.Extensions.FileProviders.IFileProvider? WebRootFileProvider { get; set; }
        }

        [GlobalSetup]
        public void Setup()
        {
            var env = new FakeEnv
            {
                ContentRootPath = Path.Combine(Path.GetTempPath(), "KvizCommando_Bench")
            };
            Directory.CreateDirectory(env.ContentRootPath);
            _logger = new AuditLogger(env);
        }

        [Benchmark]
        public async Task LogAsync()
        {
            await _logger.LogAsync("ForgotPasswordRequested", "user123", "127.0.0.1");
        }
    }
}
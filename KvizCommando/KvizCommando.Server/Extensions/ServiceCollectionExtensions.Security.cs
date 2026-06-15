using KvizCommando.Server.Application.Abstractions.Security;
using KvizCommando.Server.Application.Security;
using KvizCommando.Server.Infrastructure.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace KvizCommando.Server.Infrastructure.Extensions
{
    /// <summary>
    /// Biztonsági/PII szolgáltatások regisztrációja.
    /// ÉLETciklusok:
    /// - IEmailLookup: Singleton (stateless + config)
    /// - IEncryptionProvider: Singleton (dummy; élesben is jó, ha stateless)
    /// - IUserPiiService: Scoped (DbContext-et használ)
    /// </summary>
    public static class ServiceCollectionExtensionsSecurity
    {
        public static IServiceCollection AddSecurityAndPii(this IServiceCollection services, IConfiguration config)
        {
            services.Configure<SecurityOptions>(config.GetSection("Security"));

            services.AddSingleton<IEmailLookup, EmailLookup>();
            services.AddSingleton<IEncryptionProvider, DummyEncryptionProvider>();

            services.AddScoped<IUserPiiService, DummyUserPiiService>();

            return services;
        }
    }
}

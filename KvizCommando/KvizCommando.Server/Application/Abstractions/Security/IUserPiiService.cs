using System.Threading;
using System.Threading.Tasks;

namespace KvizCommando.Server.Application.Abstractions.Security
{
    /// <summary>
    /// PII hozzáférési szolgáltatás – minden PII művelet ezen megy át.
    /// Dummy implementációval indul, később AES-GCM-re cserélhető.
    /// </summary>
    public interface IUserPiiService
    {
        Task SetEmailAsync(string userId, string email, CancellationToken ct = default);
        Task<string?> GetEmailAsync(string userId, CancellationToken ct = default);

        Task<string?> FindUserIdByEmailAsync(string email, CancellationToken ct = default);

        Task SetPhoneAsync(string userId, string phoneE164, CancellationToken ct = default);
        Task<string?> GetPhoneAsync(string userId, CancellationToken ct = default);

        Task SetBillingAsync(string userId, string billingName, string billingAddress, CancellationToken ct = default);
        Task<(string? BillingName, string? BillingAddress)> GetBillingAsync(string userId, CancellationToken ct = default);
    }
}

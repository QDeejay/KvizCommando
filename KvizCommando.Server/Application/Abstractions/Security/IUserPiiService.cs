using System.Threading;
using System.Threading.Tasks;

namespace KvizCommando.Server.Application.Abstractions.Security
{
    /// <summary>
    /// PII hozzáférési szolgáltatás – minden PII művelet ezen megy át.
    /// A személyes adatok elkülönített tárolásának és visszakeresésének szerződése.
    /// Az éles implementációnak hitelesített titkosítást kell alkalmaznia.
    /// </summary>
    public interface IUserPiiService
    {
        /// <summary>
        /// Elmenti a felhasználó e-mail-címét.
        /// </summary>
        Task SetEmailAsync(string userId, string email, CancellationToken ct = default);
        /// <summary>
        /// Visszaadja a felhasználó tárolt e-mail-címét.
        /// </summary>
        Task<string?> GetEmailAsync(string userId, CancellationToken ct = default);

        /// <summary>
        /// Megkeresi a normalizált e-mail-hashhez tartozó felhasználóazonosítót.
        /// </summary>
        Task<string?> FindUserIdByEmailAsync(string email, CancellationToken ct = default);

        /// <summary>
        /// Elmenti a felhasználó telefonszámát.
        /// </summary>
        Task SetPhoneAsync(string userId, string phoneE164, CancellationToken ct = default);
        /// <summary>
        /// Visszaadja a felhasználó tárolt telefonszámát.
        /// </summary>
        Task<string?> GetPhoneAsync(string userId, CancellationToken ct = default);

        /// <summary>
        /// Elmenti a felhasználó számlázási adatait.
        /// </summary>
        Task SetBillingAsync(string userId, string billingName, string billingAddress, CancellationToken ct = default);
        /// <summary>
        /// Visszaadja a felhasználó tárolt számlázási adatait.
        /// </summary>
        Task<(string? BillingName, string? BillingAddress)> GetBillingAsync(string userId, CancellationToken ct = default);
    }
}

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
        /// <param name="userId">Az Identity-felhasználó azonosítója.</param>
        /// <param name="email">A mentendő e-mail-cím.</param>
        /// <param name="ct">A művelet megszakítását jelző token.</param>
        Task SetEmailAsync(string userId, string email, CancellationToken ct = default);
        /// <summary>
        /// Visszaadja a felhasználó tárolt e-mail-címét.
        /// </summary>
        /// <param name="userId">Az Identity-felhasználó azonosítója.</param>
        /// <param name="ct">A művelet megszakítását jelző token.</param>
        Task<string?> GetEmailAsync(string userId, CancellationToken ct = default);

        /// <summary>
        /// Megkeresi a normalizált e-mail-hashhez tartozó felhasználóazonosítót.
        /// </summary>
        /// <param name="email">A feldolgozandó e-mail-cím.</param>
        /// <param name="ct">A művelet megszakítását jelző token.</param>
        Task<string?> FindUserIdByEmailAsync(string email, CancellationToken ct = default);

        /// <summary>
        /// Elmenti a felhasználó telefonszámát.
        /// </summary>
        /// <param name="userId">Az Identity-felhasználó azonosítója.</param>
        /// <param name="phoneE164">A telefonszám E.164 formátumban.</param>
        /// <param name="ct">A művelet megszakítását jelző token.</param>
        Task SetPhoneAsync(string userId, string phoneE164, CancellationToken ct = default);
        /// <summary>
        /// Visszaadja a felhasználó tárolt telefonszámát.
        /// </summary>
        /// <param name="userId">Az Identity-felhasználó azonosítója.</param>
        /// <param name="ct">A művelet megszakítását jelző token.</param>
        Task<string?> GetPhoneAsync(string userId, CancellationToken ct = default);

        /// <summary>
        /// Elmenti a felhasználó számlázási adatait.
        /// </summary>
        /// <param name="userId">Az Identity-felhasználó azonosítója.</param>
        /// <param name="billingName">A számlázási név.</param>
        /// <param name="billingAddress">A számlázási cím.</param>
        /// <param name="ct">A művelet megszakítását jelző token.</param>
        Task SetBillingAsync(string userId, string billingName, string billingAddress, CancellationToken ct = default);
        /// <summary>
        /// Visszaadja a felhasználó tárolt számlázási adatait.
        /// </summary>
        /// <param name="userId">Az Identity-felhasználó azonosítója.</param>
        /// <param name="ct">A művelet megszakítását jelző token.</param>
        /// <returns>A tárolt számlázási név és cím; a hiányzó mezők értéke <see langword="null"/>.</returns>
        Task<(string? BillingName, string? BillingAddress)> GetBillingAsync(string userId, CancellationToken ct = default);
    }
}

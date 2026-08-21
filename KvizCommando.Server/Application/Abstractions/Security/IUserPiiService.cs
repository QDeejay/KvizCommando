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
        /// <summary>Atomikusan elmenti a profil kapcsolattartási és számlázási adatait; az üres mező törli a korábbi értéket.</summary>
        /// <param name="userId">Az Identity-felhasználó azonosítója.</param>
        /// <param name="phone">A telefonszám.</param>
        /// <param name="billingName">A számlázási név.</param>
        /// <param name="billingAddress">A számlázási cím.</param>
        /// <param name="ct">A művelet megszakítását jelző token.</param>
        Task SetProfileAsync(string userId, string? phone, string? billingName, string? billingAddress, CancellationToken ct = default);

        /// <summary>Visszaadja a profil titkosítva tárolt kapcsolattartási és számlázási adatait.</summary>
        /// <param name="userId">Az Identity-felhasználó azonosítója.</param>
        /// <param name="ct">A művelet megszakítását jelző token.</param>
        /// <returns>A telefonszám, számlázási név és számlázási cím.</returns>
        Task<(string? Phone, string? BillingName, string? BillingAddress)> GetProfileAsync(string userId, CancellationToken ct = default);

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

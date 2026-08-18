using KvizCommando.Shared.Contracts.Auth;
using KvizCommando.Shared.Contracts.CheckIn;


namespace KvizCommando.Client.Services.User
{
    public interface IUserService
    {
        /// <summary>
        /// Hitelesíti a felhasználót a megadott bejelentkezési adatokkal.
        /// </summary>
        /// <param name="formData">A bejelentkezési űrlap e-mail- és jelszóadatai.</param>
        /// <returns>A sikerességi jelző és sikertelenség esetén a megjelenítendő hiba.</returns>
        Task<(bool Success, string Errors)> LoginAsync(LoginRequestForm formData);
        /// <summary>
        /// Kijelentkezteti az aktuális felhasználót.
        /// </summary>
        /// <param name="soft"><see langword="true"/> esetén csak a kliens helyi állapotát zárja le; <see langword="false"/> esetén a szerveres kijelentkezést is kéri.</param>
        Task LogoutAsync(bool soft);
        /// <summary>
        /// Törli az aktuális felhasználói profilt.
        /// </summary>
        /// <returns><see langword="true"/>, ha a művelet sikeresen befejeződött; egyébként <see langword="false"/>.</returns>
        Task<bool> ProfileDeleteAsync();
        /// <summary>
        /// Regisztrálja a megadott felhasználói profilt.
        /// </summary>
        /// <param name="formData">A regisztrációs űrlap adatai.</param>
        /// <returns>A regisztráció sikeressége és a lokalizálható Identity-hibakódok.</returns>
        Task<(bool Success, List<string> Errors)> ProfileRegistAsync(RegisterRequestForm formData);
        /// <summary>
        /// Megerősíti a felhasználó e-mail-címét az Identity kódjával.
        /// </summary>
        /// <param name="userId">Az Identity-felhasználó azonosítója.</param>
        /// <param name="code">A lokalizálandó kategóriakód.</param>
        /// <returns><see langword="true"/>, ha a művelet sikeresen befejeződött; egyébként <see langword="false"/>.</returns>
        Task<bool> ConfirmEmailAsync(string userId, string code);
        /// <summary>
        /// Elindítja az elfelejtett jelszó helyreállítási folyamatát.
        /// </summary>
        /// <param name="formData">A jelszó-helyreállítást kérő űrlap adatai.</param>
        /// <returns><see langword="true"/>, ha a művelet sikeresen befejeződött; egyébként <see langword="false"/>.</returns>
        Task<bool> ForgotPswAsync(ForgotPasswordRequestForm formData);
        /// <summary>
        /// Beállítja az új jelszót a helyreállítási kód alapján.
        /// </summary>
        /// <param name="formData">A helyreállítási kódot és az új jelszót tartalmazó űrlap.</param>
        /// <returns>A jelszócsere sikeressége és a lokalizálható Identity-hibakódok.</returns>
        Task<(bool Success, List<string> Errors)> RecoverPasswordAsync(ResetPasswordForm formData);
        /// <summary>
        /// Lekéri a bejelentkezés utáni beléptetési állapotot, és szükség esetén navigál.
        /// </summary>
        /// <param name="needToRoute">Jelzi, hogy sikertelen vagy befejezett ellenőrzés után a szolgáltatás navigálhat-e.</param>
        /// <param name="ct">A művelet megszakítását jelző token.</param>
        /// <returns>A kezdőképernyőre lépés engedélye és a lokalizálható hibakódok.</returns>
        Task<(bool CanNavigateHome, List<string> Errors)> CheckInStartAsync(bool needToRoute, CancellationToken ct = default);
        /// <summary>
        /// Elküldi a beléptetési adatokat, és visszaadja a lokalizálható hibakódokat.
        /// </summary>
        /// <param name="request">A játékosnév- és ÁSZF-elfogadási adatokat tartalmazó kérés.</param>
        /// <param name="ct">A művelet megszakítását jelző token.</param>
        /// <returns>A mentés sikeressége, a lokalizálható hibakódok és az esetleges játékosnév-javaslat.</returns>
        Task<(bool Success, List<string> Errors, string SugDispName)> CheckInFinishedAsync(CheckInPostRequest request, CancellationToken ct = default);
    }
}

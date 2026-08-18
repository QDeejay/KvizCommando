using KvizCommando.Shared.Contracts.Auth;
using KvizCommando.Shared.Contracts.CheckIn;


namespace KvizCommando.Client.Services.User
{
    public interface IUserService
    {
        /// <summary>
        /// Hitelesíti a felhasználót a megadott bejelentkezési adatokkal.
        /// </summary>
        Task<(bool Success, string Errors)> LoginAsync(LoginRequestForm formData);
        /// <summary>
        /// Kijelentkezteti az aktuális felhasználót.
        /// </summary>
        Task LogoutAsync(bool soft);
        /// <summary>
        /// Törli az aktuális felhasználói profilt.
        /// </summary>
        Task<bool> ProfileDeleteAsync();
        /// <summary>
        /// Regisztrálja a megadott felhasználói profilt.
        /// </summary>
        Task<(bool Success, List<string> Errors)> ProfileRegistAsync(RegisterRequestForm formData);
        /// <summary>
        /// Megerősíti a felhasználó e-mail-címét az Identity kódjával.
        /// </summary>
        Task<bool> ConfirmEmailAsync(string userId, string code);
        /// <summary>
        /// Elindítja az elfelejtett jelszó helyreállítási folyamatát.
        /// </summary>
        Task<bool> ForgotPswAsync(ForgotPasswordRequestForm formData);
        /// <summary>
        /// Beállítja az új jelszót a helyreállítási kód alapján.
        /// </summary>
        Task<(bool Success, List<string> Errors)> RecoverPasswordAsync(ResetPasswordForm formData);
        /// <summary>
        /// Lekéri a bejelentkezés utáni beléptetési állapotot, és szükség esetén navigál.
        /// </summary>
        Task<(bool CanNavigateHome, List<string> Errors)> CheckInStartAsync(bool needToRoute, CancellationToken ct = default);
        /// <summary>
        /// Elküldi a beléptetési adatokat, és visszaadja a lokalizálható hibakódokat.
        /// </summary>
        Task<(bool Success, List<string> Errors, string SugDispName)> CheckInFinishedAsync(CheckInPostRequest request, CancellationToken ct = default);
    }
}

using KvizCommando.Shared.Contracts.Auth;
using KvizCommando.Shared.Contracts.CheckIn;


namespace KvizCommando.Client.Services.User
{
    public interface IUserService
    {
        Task<(bool Success, string Errors)> LoginAsync(LoginRequestForm formData);
        Task LogoutAsync(bool soft);
        Task<bool> ProfileDeleteAsync();
        Task<(bool Success, List<string> Errors)> ProfileRegistAsync(RegisterRequestForm formData);
        Task<bool> ConfirmEmailAsync(string userId, string code);
        Task<bool> ForgotPswAsync(ForgotPasswordRequestForm formData);
        Task<(bool Success, List<string> Errors)> RecoverPasswordAsync(ResetPasswordForm formData);
        Task<(bool Success, List<string> Errors)> CheckInStartAsync(bool needToRoute,CancellationToken ct = default);
        Task<(bool Success, List<string> Errors, string SugDispName)> CheckInFinishedAsync(CheckInPostRequest request, CancellationToken ct = default);
    }
}

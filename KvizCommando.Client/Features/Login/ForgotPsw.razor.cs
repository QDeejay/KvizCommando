using KvizCommando.Client.Services;
using KvizCommando.Client.Services.User;
using KvizCommando.Client.Utilities;
using KvizCommando.Localization.Login;
using KvizCommando.Shared.Contracts.Auth;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace KvizCommando.Client.Features.Login
{
    partial class ForgotPsw : KcComponentBase
    {
        [Inject] private IdentityRulesService IdentityRules { get; set; } = default!;
        [Inject] private IStringLocalizer<LoginResource> Lang { get; set; } = default!;


        private ForgotPasswordRequestForm formData { get; set; } = new();
        private string ResultMessage { get; set; } = string.Empty;
        private bool ColorSW { get; set; } = false;
        private bool CanSend => !string.IsNullOrWhiteSpace(formData.email);
        private bool Success { get; set; } = false;
        private RegisterOptionsResponse? _options;
        private string PasswordResetEmailOutputPath =>
            _options?.PasswordResetEmailOutputPath ?? string.Empty;

        protected override async Task OnInitializedAsync()
        {
            _options = await IdentityRules.GetRulesAsync();
        }

        protected async Task SendEmail()
        {
            if (!IsValidEmail(formData.email))
            {
                ResultMessage = Lang["forgotosw.Error.Email"];
                ColorSW = true;
                return;
            }
            ColorSW = false;
            await User.ForgotPswAsync(formData);
            Success = true;
            ResultMessage = Lang["forgotosw.Succes.Email"];
            formData.email = string.Empty;
        }
        private void NavigateHome()
        {
            Ui.Nav.NavigateTo("/login");
        }
        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}

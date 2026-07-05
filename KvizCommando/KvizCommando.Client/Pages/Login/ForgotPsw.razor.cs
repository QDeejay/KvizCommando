using KvizCommando.Client.Services.User;
using KvizCommando.Client.Services.Visual.UiService.Language;
using KvizCommando.Client.Utilities;
using KvizCommando.Shared.Contracts.Auth;
using Microsoft.AspNetCore.Components;
using System.Globalization;

namespace KvizCommando.Client.Pages.Login
{
    partial class ForgotPsw : KcComponentBase
    {
        readonly string culture = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;

        private ForgotPasswordRequestForm formData { get; set; } = new();
        private string ResultMessage { get; set; } = string.Empty;
        private bool ColorSW { get; set; } = false;
        private bool CanSend => !string.IsNullOrWhiteSpace(formData.email);
        private bool Success { get; set; } = false;
        protected async Task SendEmail()
        {
            if (!IsValidEmail(formData.email))
            {
                ResultMessage = Ui.Lang["forgotosw.Error.Email"];
                ColorSW = true;
                return;
            }
            else
            {
                ColorSW = false;
                await User.ForgotPswAsync(formData);
                    Success = true;
                    ResultMessage = Ui.Lang["forgotosw.Succes.Email"];
                    formData.email = string.Empty;
                    return;  
            }

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
        protected override async Task OnInitializedAsync()
        {
            await Task.Delay(5);
        }
    }
}

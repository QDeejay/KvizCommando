using KvizCommando.Client.Services.Language;
using KvizCommando.Client.Services.User;
using KvizCommando.Shared.Contracts.Auth;
using Microsoft.AspNetCore.Components;
using System.Globalization;

namespace KvizCommando.Client.Pages.Login
{
    partial class ForgotPsw : ComponentBase
    {

        [Inject] private NavigationManager Nav { get; set; } = default!;
        [Inject] private ILanguageService Lang { get; set; } = default!;
        [Inject] private IUserService Service { get; set; } = default!;

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
                ResultMessage = Lang["forgotosw.Error.Email"];
                ColorSW = true;
                return;
            }
            else
            {
                ColorSW = false;
                var response = await Service.ForgotPswAsync(formData);
                if (response != null)
                {
                    Success = true;
                    ResultMessage = Lang["forgotosw.Succes.Email"];
                    formData.email = string.Empty;
                    return;
                }
                else 
                {
                    return;
                }
                    
            }

        }
        private void NavigateHome()
        {
            Nav.NavigateTo("/");
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

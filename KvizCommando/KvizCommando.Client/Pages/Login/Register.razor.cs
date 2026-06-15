using KvizCommando.Client.Helpers;
using KvizCommando.Client.Services;
using KvizCommando.Client.Services.Language;
using KvizCommando.Client.Services.User;
using KvizCommando.Shared.Contracts.Auth;
using KvizCommando.Shared.Options;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Text.RegularExpressions;

namespace KvizCommando.Client.Pages.Login
{
    public partial class Register : ComponentBase
    {
        [Inject] private NavigationManager Nav { get; set; } = default!;
        [Inject] private IdentityRulesService IdentityRules { get; set; } = default!;
        [Inject] private ILanguageService Lang { get; set; } = default!;
        [Inject] public IUserService UserService { get; set; } = default!;

        string culture = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
        private RegisterRequestForm FormData { get; set; } = new();

        private RegisterOptionsResponse? Options { get; set; }
        private string ResultMessage { get; set; } = string.Empty;
        private bool ShowValidation { get; set; } = false;
        private bool EmailFiledSW { get; set; } = false;
        private bool PasswordFiledSW { get; set; } = false;
        private bool CanRegister =>
            !string.IsNullOrWhiteSpace(FormData.ConfirmPassword)
            && !string.IsNullOrWhiteSpace(FormData.Email)
            && !string.IsNullOrWhiteSpace(FormData.Password);

        private async Task HandleValidSubmit()
        {
            ShowValidation = true;
            ResultMessage = null;
            
            // --- Email ---
            if (!IsValidEmail(FormData.Email))
            {
                ResultMessage = Lang["identityerrors.InvalidEmail"].FormatSafe(FormData.Email);
                EmailFiledSW = true;
            }
            else
            {
                EmailFiledSW = false;
            }

            // --- Password: IdentityOptions alapján teljes ellenőrzés ---
            PasswordFiledSW = false;
            if (Options is not null)
            {
                var pwd = FormData.Password ?? string.Empty;

                if (pwd.Length < Options.RequiredLength)
                {
                    ResultMessage = Lang["identityerrors.PasswordTooShort"].FormatSafe(Options.RequiredLength);
                    PasswordFiledSW = true;
                }
                else if (Options.RequireDigit && !pwd.Any(char.IsDigit))
                {
                    ResultMessage = Lang["identityerrors.PasswordRequiresDigit"];
                    PasswordFiledSW = true;
                }
                else if (Options.RequireLowercase && !pwd.Any(char.IsLower))
                {
                    ResultMessage = Lang["identityerrors.PasswordRequiresLower"];
                    PasswordFiledSW = true;
                }
                else if (Options.RequireUppercase && !pwd.Any(char.IsUpper))
                {
                    ResultMessage = Lang["identityerrors.PasswordRequiresUpper"];
                    PasswordFiledSW = true;
                }
                else if (Options.RequireNonAlphanumeric && pwd.All(char.IsLetterOrDigit))
                {
                    ResultMessage = Lang["identityerrors.PasswordRequiresNonAlphanumeric"];
                    PasswordFiledSW = true;
                }
                else if (Options.RequiredUniqueChars > 1 && pwd.Distinct().Count() < Options.RequiredUniqueChars)
                {
                    ResultMessage = Lang["identityerrors.PasswordRequiresUniqueChars"].FormatSafe(Options.RequiredLength);
                    PasswordFiledSW = true;
                }
                else if (FormData.Password != FormData.ConfirmPassword)
                {
                    ResultMessage = Lang["identityerrors.PasswordNotMatched"];
                    PasswordFiledSW = true;
                }
            }
           

            // Ha bármelyik mező hibás, ne küldjük a szerverre
            if (EmailFiledSW || PasswordFiledSW)
                return;

            
            // Kérés a szerver felé
            var (success, errors) = await UserService.ProfileRegistAsync(FormData);

            if (success)
            {
                Nav.NavigateTo("/registersucces");
                return;
            }

            // Hibák kezelése: a szerver identityerrors.* kulcsokat ad vissza
            if (errors is { Count: > 0 })
            {
                // csak az első hibát mutatjuk; ha több kell, join-olható
                ResultMessage = Lang[$"identityerrors.{errors[0]}"];
            }
            else
            {
                ResultMessage = Lang["identityerrors.DefaultError"];
            }
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
        private void NavigateHome()
        {
            Nav.NavigateTo("/");
        }
        protected override async Task OnInitializedAsync()
        {
            culture = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
            Options = await IdentityRules.GetRulesAsync();
        }
    }
}
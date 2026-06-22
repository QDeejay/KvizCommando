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
    public partial class Register : ComponentBase, IDisposable
    {
        [Inject] private NavigationManager Nav { get; set; } = default!;
        [Inject] private IdentityRulesService IdentityRules { get; set; } = default!;
        [Inject] private ILanguageService Lang { get; set; } = default!;
        [Inject] private IUserService UserService { get; set; } = default!;

        private readonly RegisterRequestForm FormData = new();
        private RegisterOptionsResponse? Options = default!;
        private string _resultMessage  = string.Empty;
        private bool _emailFiledSW = false;
        private bool _passwordFiledSW = false;

        private string _culture = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
        private bool CanRegister =>
            !string.IsNullOrWhiteSpace(FormData.ConfirmPassword)
            && !string.IsNullOrWhiteSpace(FormData.Email)
            && !string.IsNullOrWhiteSpace(FormData.Password);

        private async Task HandleValidSubmit()
        {
            _showValidation = true;
            _resultMessage = string.Empty;
            
            // --- Email ---
            if (!LoginHelper.IsValidEmail(FormData.Email))
            {
                _resultMessage = Lang["identityerrors.InvalidEmail"].FormatSafe(FormData.Email);
                _emailFiledSW = true;
            }
            else
            {
                _emailFiledSW = false;
            }

            // --- Password: IdentityOptions alapján teljes ellenőrzés ---
            _passwordFiledSW = false;
            if (Options is not null)
            {
                var pwd = FormData.Password ?? string.Empty;

                if (pwd.Length < Options.RequiredLength)
                {
                    _resultMessage = Lang["identityerrors.PasswordTooShort"].FormatSafe(Options.RequiredLength);
                    _passwordFiledSW = true;
                }
                else if (Options.RequireDigit && !pwd.Any(char.IsDigit))
                {
                    _resultMessage = Lang["identityerrors.PasswordRequiresDigit"];
                    _passwordFiledSW = true;
                }
                else if (Options.RequireLowercase && !pwd.Any(char.IsLower))
                {
                    _resultMessage = Lang["identityerrors.PasswordRequiresLower"];
                    _passwordFiledSW = true;
                }
                else if (Options.RequireUppercase && !pwd.Any(char.IsUpper))
                {
                    _resultMessage = Lang["identityerrors.PasswordRequiresUpper"];
                    _passwordFiledSW = true;
                }
                else if (Options.RequireNonAlphanumeric && pwd.All(char.IsLetterOrDigit))
                {
                    _resultMessage = Lang["identityerrors.PasswordRequiresNonAlphanumeric"];
                    _passwordFiledSW = true;
                }
                else if (Options.RequiredUniqueChars > 1 && pwd.Distinct().Count() < Options.RequiredUniqueChars)
                {
                    _resultMessage = Lang["identityerrors.PasswordRequiresUniqueChars"].FormatSafe(Options.RequiredLength);
                    _passwordFiledSW = true;
                }
                else if (FormData.Password != FormData.ConfirmPassword)
                {
                    _resultMessage = Lang["identityerrors.PasswordNotMatched"];
                    _passwordFiledSW = true;
                }
            }
           

            // Ha bármelyik mező hibás, ne küldjük a szerverre
            if (_emailFiledSW || _passwordFiledSW)
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
                _resultMessage = Lang[$"identityerrors.{errors[0]}"];
            }
            else
            {
                _resultMessage = Lang["identityerrors.DefaultError"];
            }
        }
        private void NavigateHome()
        {
            Nav.NavigateTo("/login");
        }
        protected override async Task OnInitializedAsync()
        {
            _culture = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
            Options = await IdentityRules.GetRulesAsync();
        }
        public void Dispose() { }
    }
}
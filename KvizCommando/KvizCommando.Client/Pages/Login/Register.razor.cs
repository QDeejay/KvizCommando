using KvizCommando.Client.Helpers;
using KvizCommando.Client.Services;
using KvizCommando.Client.Services.User;
using KvizCommando.Client.Services.Visual.UiService.Language;
using KvizCommando.Client.Utilities;
using KvizCommando.Shared.Contracts.Auth;
using KvizCommando.Shared.Options;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Text.RegularExpressions;

namespace KvizCommando.Client.Pages.Login
{
    public partial class Register : KcComponentBase, IDisposable
    {
        //[Inject] private NavigationManager Nav { get; set; } = default!;
        [Inject] private IdentityRulesService IdentityRules { get; set; } = default!;
        //[Inject] private ILanguageService Lang { get; set; } = default!;
        //[Inject] private IUserService UserService { get; set; } = default!;

        private readonly RegisterRequestForm _formData = new();
        private RegisterOptionsResponse? _options = default!;
        private string _resultMessage = string.Empty;
        private bool _emailFiledSW = false;
        private bool _passwordFiledSW = false;
        private bool _registSucces = false;
        private bool[] _showPassword = new bool[2];
        private string PasswordType1 => _showPassword[0] ? "text" : "password";
        private string PasswordType2 => _showPassword[1] ? "text" : "password";
        private string EyeIcon1 =>
            _showPassword[0] ? "bi bi-eye-slash" : "bi bi-eye";
        private string EyeIcon2 =>
           _showPassword[1] ? "bi bi-eye-slash" : "bi bi-eye";

        private void TogglePassword(int pw)
        {
             _showPassword[pw] = !_showPassword[pw];
        }
        private string _culture = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
        private bool CanRegister =>
            !string.IsNullOrWhiteSpace(_formData.ConfirmPassword)
            && !string.IsNullOrWhiteSpace(_formData.Email)
            && !string.IsNullOrWhiteSpace(_formData.Password);

        private async Task HandleValidSubmit()
        {
            //_showValidation = true;
            _resultMessage = string.Empty;
            
            // --- Email ---
            if (!LoginHelper.IsValidEmail(_formData.Email))
            {
                _resultMessage = Ui.Lang["identityerrors.InvalidEmail"].FormatSafe(_formData.Email);
                _emailFiledSW = true;
            }
            else
            {
                _emailFiledSW = false;
            }

            // --- Password: IdentityOptions alapján teljes ellenőrzés ---
            _passwordFiledSW = false;
            if (_options is not null)
            {
                var pwd = _formData.Password ?? string.Empty;

                if (pwd.Length < _options.RequiredLength)
                {
                    _resultMessage = Ui.Lang["identityerrors.PasswordTooShort"].FormatSafe(_options.RequiredLength);
                    _passwordFiledSW = true;
                }
                else if (_options.RequireDigit && !pwd.Any(char.IsDigit))
                {
                    _resultMessage = Ui.Lang["identityerrors.PasswordRequiresDigit"];
                    _passwordFiledSW = true;
                }
                else if (_options.RequireLowercase && !pwd.Any(char.IsLower))
                {
                    _resultMessage = Ui.Lang["identityerrors.PasswordRequiresLower"];
                    _passwordFiledSW = true;
                }
                else if (_options.RequireUppercase && !pwd.Any(char.IsUpper))
                {
                    _resultMessage = Ui.Lang["identityerrors.PasswordRequiresUpper"];
                    _passwordFiledSW = true;
                }
                else if (_options.RequireNonAlphanumeric && pwd.All(char.IsLetterOrDigit))
                {
                    _resultMessage = Ui.Lang["identityerrors.PasswordRequiresNonAlphanumeric"];
                    _passwordFiledSW = true;
                }
                else if (_options.RequiredUniqueChars > 1 && pwd.Distinct().Count() < _options.RequiredUniqueChars)
                {
                    _resultMessage = Ui.Lang["identityerrors.PasswordRequiresUniqueChars"].FormatSafe(_options.RequiredLength);
                    _passwordFiledSW = true;
                }
                else if (_formData.Password != _formData.ConfirmPassword)
                {
                    _resultMessage = Ui.Lang["identityerrors.PasswordNotMatched"];
                    _passwordFiledSW = true;
                }
            }
           

            // Ha bármelyik mező hibás, ne küldjük a szerverre
            if (_emailFiledSW || _passwordFiledSW)
                return;

            
            // Kérés a szerver felé
            var (success, errors) = await User.ProfileRegistAsync(_formData);

            if (success)
            {
                _registSucces = true;
                return;
            }

            // Hibák kezelése: a szerver identityerrors.* kulcsokat ad vissza
            if (errors is { Count: > 0 })
            {
                // csak az első hibát mutatjuk; ha több kell, join-olható
                _resultMessage = Ui.Lang[$"identityerrors.{errors[0]}"];
            }
            else
            {
                _resultMessage = Ui.Lang["identityerrors.DefaultError"];
            }
        }
        private void NavigateHome()
        {
            Ui.Nav.NavigateTo("/login");
        }
        protected override async Task OnInitializedAsync()
        {
             _registSucces = false;
             _culture = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
             _options = await IdentityRules.GetRulesAsync();
        }
        public void Dispose() { }
    }
}
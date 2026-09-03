using KvizCommando.Client.Helpers;
using KvizCommando.Client.Services;
using KvizCommando.Client.Services.User;
using KvizCommando.Client.Utilities;
using KvizCommando.Shared.Contracts.Auth;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System.Globalization;



namespace KvizCommando.Client.Features.Login
{
    public partial class Login : KcComponentBase
    {
        [Inject] private IdentityRulesService IdentityRules { get; set; } = default!;

        private RegisterOptionsResponse? _options;
        private bool _enterPassPage = false;
        private bool _invalidEmail = false;
        private string _maskedEmail = string.Empty;
        private string _errorMessage = string.Empty;
        private ElementReference passwordInput;
        private bool _showPassword;

        private string _passwordType => _showPassword ? "text" : "password";

        private string EyeIcon =>
            _showPassword ? "bi bi-eye-slash" : "bi bi-eye";

        private void TogglePassword()
        {
            _showPassword = !_showPassword;
        }

        private readonly LoginRequestForm _loginForm = new();

        private static string Culture => CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
        private bool CanNext => !string.IsNullOrWhiteSpace(_loginForm.Email);
        private bool CanLogin => !string.IsNullOrWhiteSpace(_loginForm.Email)
                              && !string.IsNullOrWhiteSpace(_loginForm.Password);
        private bool RegistrationEnabled => _options?.RegistrationEnabled == true;
        private bool FacebookLoginEnabled => _options?.FacebookLoginEnabled == true;
        private string InvitationTestPeriod => _options?.InvitationTestPeriod ?? string.Empty;
        private string ApplicationEmailHref =>
            $"mailto:{_options?.SupportEmail ?? string.Empty}?subject={Uri.EscapeDataString(Ui.Lang["login.InvitationOnly.EmailSubject"])}";

        private async Task OnSwitchPass(bool viaEnter)
        {
            _invalidEmail = false;
            _errorMessage = string.Empty;
            var valid = LoginHelper.IsValidEmail(_loginForm.Email);
            if (valid)
            {
                _enterPassPage = !_enterPassPage;
                _loginForm.Password = string.Empty;
                _showPassword = false;
                _maskedEmail = LoginHelper.MaskEmail(_loginForm.Email);
                StateHasChanged();
                if (viaEnter)
                {
                    await Task.Yield();
                    await passwordInput.FocusAsync();
                }
                
            }
            else
            {
                _errorMessage = Ui.Lang["identityerrors.InvalidEmail"].FormatSafe(_loginForm.Email);
                _invalidEmail = true;
                _ =  ShowError();
            }
        }
        private async Task LoginUser()
        {
            try
            {
                var response = await User.LoginAsync(_loginForm);

                if (response.Success)
                {
                    await ContinueAfterLoginAsync();
                }
                else
                {
                    _errorMessage = Ui.Lang[response.Errors];

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Login failed: {ex.Message}");
                _errorMessage = Ui.Lang["identityerrors.default"];
            }
        }
        private async Task ContinueAfterLoginAsync()
        {
            var result = await User.CheckInStartAsync(true);

            if (result.CanNavigateHome)
            {
                Ui.Nav.NavigateTo("/home");
                return;
            }

            if (result.Errors.Count > 0)
                _errorMessage = Ui.Lang["identityerrors.DefaultError"];
        }
        private async Task OnKeyDown(KeyboardEventArgs e)
        {
            if (e.Key == "Enter"  )
            {
                if (CanNext && !_enterPassPage)
                    await OnSwitchPass(true);
                else if (CanLogin && _enterPassPage)
                    await LoginUser();
            }
        }
        private async Task ShowError()
        {
            await Task.Delay(1000);
            _errorMessage = string.Empty;
        }
        protected override async Task OnInitializedAsync()
        {
            _enterPassPage = false;
            var uri = Ui.Nav.ToAbsoluteUri(Ui.Nav.Uri);
            var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
            var Error = query["error"];

            // (opcionális) takarítsd le az URL-ből az ?error-t a címsorból:
            if (!string.IsNullOrEmpty(Error))
            {
                Ui.Nav.NavigateTo(uri.GetLeftPart(UriPartial.Path), replace: true);
                _errorMessage = Ui.Lang[$"identityerrors.{Error}"];
            }

            _options = await IdentityRules.GetRulesAsync();
        }
    }
}

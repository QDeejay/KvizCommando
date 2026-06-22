using KvizCommando.Client.Helpers;
using KvizCommando.Client.Services.Language;
using KvizCommando.Client.Services.User;
using KvizCommando.Shared.Contracts.Auth;
using Microsoft.AspNetCore.Components;
using System.Globalization;



namespace KvizCommando.Client.Pages.Login
{
    public partial class Login : ComponentBase, IDisposable
    {
        [Inject] private IUserService UserService { get; set; } = default!;
        [Inject] private ILanguageService Lang { get; set; } = default!;
        [Inject] private NavigationManager Nav { get; set; } = default!;

        private bool _enterPassPage = false;
        private bool _invalidEmail = false;
        private string _maskedEmail = string.Empty;
        private string _errorMessage = string.Empty;

        private readonly LoginRequestForm LoginForm = new();

        private static string Culture => CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
        private bool CanNext => !string.IsNullOrWhiteSpace(LoginForm.Email);
        private bool CanLogin => !string.IsNullOrWhiteSpace(LoginForm.Email)
                              && !string.IsNullOrWhiteSpace(LoginForm.Password);

        private async Task OnSwitchPass()
        {
            _invalidEmail = false;
            var valid = LoginHelper.IsValidEmail(LoginForm.Email);
            if (valid)
            {
                _enterPassPage = !_enterPassPage;

                LoginForm.Password = string.Empty;
                _maskedEmail = LoginHelper.MaskEmail(LoginForm.Email);
            }
            else
            {
                _errorMessage = Lang["identityerrors.InvalidEmail"].FormatSafe(LoginForm.Email);
                _invalidEmail = true;
                await Task.Delay(1000);
            }
            _errorMessage = string.Empty;
        }
        private async Task LoginUser()
        {
            try
            {
                var response = await UserService.LoginAsync(LoginForm);

                if (response.Success)
                {
                    var resp = await UserService.CheckInStartAsync(true);
                    _errorMessage = Lang[response.Errors];

                }
                else
                {
                    _errorMessage = Lang[response.Errors];

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Login failed: {ex.Message}");
                _errorMessage = Lang["identityerrors.default"];
            }
            finally
            {

            }
        }
        private async Task BypassLogin()
        {
            LoginForm.Email = "qleedeejay@freemial.hu";
            LoginForm.Password = "-Ranger1980-0621";

            try
            {
                var response = await UserService.LoginAsync(LoginForm);

                if (response.Success)
                {
                    var resp = await UserService.CheckInStartAsync(true);
                    _errorMessage = Lang[response.Errors];
                }
                else
                {
                    _errorMessage = Lang[response.Errors] ?? Lang["identityerrors.default"];
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Login failed: {ex.Message}");
                _errorMessage = Lang["identityerrors.default"];
            }
            finally
            {

            }
        }

        protected override async Task OnInitializedAsync()
        {
            _enterPassPage = false;
            var uri = Nav.ToAbsoluteUri(Nav.Uri);
            var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
            var Error = query["error"];

            // (opcionális) takarítsd le az URL-ből az ?error-t a címsorból:
            if (!string.IsNullOrEmpty(Error))
            {
                Nav.NavigateTo(uri.GetLeftPart(UriPartial.Path), replace: true);
                _errorMessage = Lang[$"identityerrors.{Error}"];
            }
            await Task.Delay(5);
        }
        public void Dispose()
        {
        }
    }
}

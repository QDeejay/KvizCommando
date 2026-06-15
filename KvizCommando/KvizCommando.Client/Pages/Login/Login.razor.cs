using Blazored.SessionStorage;
using KvizCommando.Client.Models.StoreModels;
using KvizCommando.Client.Services.Audio;
using KvizCommando.Client.Services.Language;
using KvizCommando.Client.Services.User;
using KvizCommando.Client.Services.Visual;
using KvizCommando.Shared.Contracts.Auth;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;



namespace KvizCommando.Client.Pages.Login
{
    public partial class Login : ComponentBase
    {
        [Inject] private IUserService UserService { get; set; } = default!;
        [Inject] private ILoadingService Loader { get; set; } = default!;
        [Inject] private ILanguageService Lang { get; set; } = default!;
        [Inject] private NavigationManager Nav { get; set; } = default!;
     


        string culture = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;

        protected LoginRequestForm LoginForm = new ();
      
        protected string ErrorMessage { get; set; } = string.Empty;
        protected bool RememberMe { get; set; } = false;
       
        private bool CanLogin => !string.IsNullOrWhiteSpace(LoginForm.Email)
                              && !string.IsNullOrWhiteSpace(LoginForm.Password);

        protected async Task LoginUser()
        {
            try
            {
                await Loader.Show();

                var response = await UserService.LoginAsync(LoginForm);

                if (response.Success)
                {                   
                    var resp = await UserService.CheckInStartAsync(true);
                    ErrorMessage = Lang[response.Errors];
                    
                }
                else
                {
                    ErrorMessage = Lang[response.Errors];  
                      
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Login failed: {ex.Message}");
                ErrorMessage = Lang["identityerrors.default"];
            }
            finally
            {
               await Loader.Hide();
            }

        }
        protected async Task BypassLogin()
        {
            LoginForm.Email = "qleedeejay@freemial.hu";
            LoginForm.Password = "-Ranger1980-0621";
            await Loader.Show();
            try
            {
               
                
                var response = await UserService.LoginAsync(LoginForm);

                if (response.Success)
                {  
                    var resp = await UserService.CheckInStartAsync(true);
                    ErrorMessage = Lang[response.Errors];
                }
                else
                {
                    ErrorMessage = Lang[response.Errors] ?? Lang["identityerrors.default"];
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Login failed: {ex.Message}");
                ErrorMessage = Lang["identityerrors.default"];
            }
            finally
            {
                await Loader.Hide();
            }
        }

        protected override async Task OnInitializedAsync()
        {
            var uri = Nav.ToAbsoluteUri(Nav.Uri);
            var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
            var Error = query["error"];

            // (opcionális) takarítsd le az URL-ből az ?error-t a címsorból:
            if (!string.IsNullOrEmpty(Error)) 
            {
                Nav.NavigateTo(uri.GetLeftPart(UriPartial.Path), replace: true);
                ErrorMessage = Lang[$"identityerrors.{Error}"];
            }
            await Task.Delay(5);
           
            //await Sound.Load("click", "audio/click.webm");
        }

    }
}

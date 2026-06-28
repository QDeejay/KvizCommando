using KvizCommando.Client.Services.User;
using KvizCommando.Client.Services.Visual.UiService.Language;
using KvizCommando.Client.Utilities;
using Microsoft.AspNetCore.Components;
using System.Globalization;

namespace KvizCommando.Client.Pages.Login.Auth;

public partial class Confirm : KcComponentBase
    {
       // [Inject] private NavigationManager Nav { get; set; } = default!;
        //[Inject] private ILanguageService Lang { get; set; } = default!;
        //[Inject] private IUserService Service { get; set; } = default!;


        private string _culture = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;

        private bool isLoading = true;
        private bool? success = null;

        protected override async Task OnInitializedAsync()
        {

            var uri = Ui.Nav.ToAbsoluteUri(Ui.Nav.Uri);
            var query = System.Web.HttpUtility.ParseQueryString(uri.Query);

            var userId = query["userId"];
            var code = query["code"];
            isLoading = true;
            success = await User.ConfirmEmailAsync(userId!, code!);
            isLoading = false;

        }
        private void NavigateHome()
        {
            Ui.Nav.NavigateTo("/login");
        }
    }


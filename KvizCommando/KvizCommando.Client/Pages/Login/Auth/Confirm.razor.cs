using KvizCommando.Client.Services.Language;
using KvizCommando.Client.Services.User;
using Microsoft.AspNetCore.Components;
using System.Globalization;

namespace KvizCommando.Client.Pages.Login.Auth;

public partial class Confirm : ComponentBase
    {
        [Inject] private NavigationManager Nav { get; set; } = default!;
        [Inject] private ILanguageService Lang { get; set; } = default!;
        [Inject] private IUserService Service { get; set; } = default!;


        private string culture = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;

        private bool isLoading = true;
        private bool? success = null;

        protected override async Task OnInitializedAsync()
        {

            var uri = Nav.ToAbsoluteUri(Nav.Uri);
            var query = System.Web.HttpUtility.ParseQueryString(uri.Query);

            var userId = query["userId"];
            var code = query["code"];
            isLoading = true;
            success = await Service.ConfirmEmailAsync(userId!, code!);
            isLoading = false;

        }
        private void NavigateHome()
        {
            Nav.NavigateTo("/");
        }
    }


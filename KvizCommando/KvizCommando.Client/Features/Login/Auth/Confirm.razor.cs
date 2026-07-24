using KvizCommando.Client.Services.User;
using KvizCommando.Client.Services.Visual.UiService.Language;
using KvizCommando.Client.Utilities;
using Microsoft.AspNetCore.Components;
using System.Globalization;

namespace KvizCommando.Client.Features.Login.Auth;

public partial class Confirm : KcComponentBase
    {
       // [Inject] private NavigationManager Nav { get; set; } = default!;
        //[Inject] private ILanguageService Lang { get; set; } = default!;
        //[Inject] private IUserService Service { get; set; } = default!;


        private readonly string _culture = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;

        private bool _isLoading = true;
        private bool? _isSuccess = null;

        protected override async Task OnInitializedAsync()
        {

            var uri = Ui.Nav.ToAbsoluteUri(Ui.Nav.Uri);
            var query = System.Web.HttpUtility.ParseQueryString(uri.Query);

            var userId = query["userId"];
            var code = query["code"];
            _isLoading = true;
            _isSuccess = await User.ConfirmEmailAsync(userId!, code!);
            _isLoading = false;

        }
        private void NavigateHome()
        {
            Ui.Nav.NavigateTo("/login");
        }
    }


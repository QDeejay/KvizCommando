using KvizCommando.Client.Services.User;
using KvizCommando.Client.Services.Visual.UiService.Language;
using KvizCommando.Client.Utilities;
using KvizCommando.Localization.Login;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace KvizCommando.Client.Features.Login.Auth;

public partial class Confirm : KcComponentBase
    {
        [Inject] private IStringLocalizer<LoginResource> Lang { get; set; } = default!;

        private bool _isLoading = true;
        private bool? _isSuccess = null;
        private bool _isEmailChange;

        protected override async Task OnInitializedAsync()
        {

            var uri = Ui.Nav.ToAbsoluteUri(Ui.Nav.Uri);
            var query = System.Web.HttpUtility.ParseQueryString(uri.Query);

            var userId = query["userId"];
            var code = query["code"];
            var changedEmail = query["changedEmail"];
            _isEmailChange = !string.IsNullOrWhiteSpace(changedEmail);
            _isLoading = true;
            _isSuccess = await User.ConfirmEmailAsync(userId!, code!, changedEmail);
            _isLoading = false;

        }
        private void NavigateHome()
        {
            Ui.Nav.NavigateTo("/login");
        }
    }

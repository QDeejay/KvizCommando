using Blazored.LocalStorage;
using Blazored.SessionStorage;
using KvizCommando.Client.Data;
using KvizCommando.Client.Features.Shared.Help;
using KvizCommando.Client.Features.Shared.Modal;
using KvizCommando.Client.Features.Shared.Modal.Builders;
using KvizCommando.Client.Features.Solo.Builders;
using KvizCommando.Client.Features.VsGame.Builders;
using KvizCommando.Client.Helpers;
using KvizCommando.Client.Services.Audio;
using KvizCommando.Client.Services.ClientCache;
using KvizCommando.Client.Services.Visual.UiService;
using KvizCommando.Client.Utilities;
using KvizCommando.Shared.Models.Dtos;
using Microsoft.AspNetCore.Components;
using System.Globalization;

namespace KvizCommando.Client.Layout
{
    partial class MainLayout
    {
        private void ToggleDesktopSidebar()
        {
            if (CanToggleSidebar)
                _isDesktopNavOpen = !_isDesktopNavOpen;
        }

        private void ToggleMobileSidebar()
        {
            if (CanToggleSidebar)
                _isMobileNavOpen = !_isMobileNavOpen;
        }

        private void CloseMobileSidebar() => _isMobileNavOpen = false;

        private bool ShowLogoutToast(Uri uri)
        {
            var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
            var reason = query["reason"];

            if (string.IsNullOrWhiteSpace(reason))
                return false;

            Ui.Nav.NavigateTo(uri.GetLeftPart(UriPartial.Path), replace: true);

            switch (reason.ToLowerInvariant())
            {
                case "success":
                    Ui.Toast.Complete(Ui.Lang["mainlayout.Toast.Logout.Success"]);
                    break;

                case "session":
                    Ui.Toast.Brief(Ui.Lang["mainlayout.Toast.Logout.Session"]);

                    break;

                case "expired":
                    Ui.Toast.Brief(Ui.Lang["mainlayout.Toast.Logout.Expired"]);
                    break;

                case "error":
                    Ui.Toast.Error(Ui.Lang["mainlayout.Toast.Logout.Error"]);
                    break;
            }
            return true;
        }

        private void UpdateTitle()
        {
            _currentTitle = Ui.Header.Title;

            if (IsFullScreenGame)
                _isMobileNavOpen = false;

            InvokeAsync(StateHasChanged);
        }
        private void UpdateBackBtnEna()
        {
            _isBckBtnEna = Ui.Header.BackEna;
            InvokeAsync(StateHasChanged);
        }

        private void OnBackClick()
        {
            Ui.SubHeader.Hide();
            Ui.Header.SetBackBtnToPushState();
        }
        private void ShowModal() => _ = _mainModal!.ShowAsync(Ui.Modal.Parameter!);
        private void HideModal() => _ = _mainModal!.HideAsync();
        private void Refresh() => InvokeAsync(StateHasChanged);
        private void ModalAction(ModalResult result) => Ui.Modal.SendResult(result);

        private async Task SetSoundEnabledAsync(bool enabled)
        {
            await Audio.SetMutedAsync(!enabled);
        }
        private async Task Logout()
        {
            await User.LogoutAsync(false);
            Console.WriteLine("User logged out.");
        }
        private Task OpenHelpAsync() =>
            _helpNavigator?.ShowManualAsync() ?? Task.CompletedTask;
        private Task OpenCurrentHelpAsync() =>
            _helpNavigator?.ShowCurrentAsync() ?? Task.CompletedTask;

        private Task OnRefreshRequired(ReqStates[] reqTypes) =>
            InitStatesAsync(reqTypes);
    }
}

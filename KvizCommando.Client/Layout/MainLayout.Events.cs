using KvizCommando.Client.Services.Audio;
using KvizCommando.Client.Services.Visual.UiService;

namespace KvizCommando.Client.Layout
{
    partial class MainLayout
    {
        private async Task ToggleDesktopSidebar()
        {
            if (CanToggleSidebar)
            {
                await Audio.PlaySfxAsync(AudioService.SFX_UI_TOUCH);
                _isDesktopNavOpen = !_isDesktopNavOpen;
            }
        }

        private async Task ToggleMobileSidebar()
        {
            if (CanToggleSidebar)
            {
                await Audio.PlaySfxAsync(AudioService.SFX_UI_TOUCH);
                _isMobileNavOpen = !_isMobileNavOpen;
            }
        }

        private void CloseMobileSidebar() => _isMobileNavOpen = false;

        private async Task CloseMobileSidebarFromBackdropAsync()
        {
            await Audio.PlaySfxAsync(AudioService.SFX_UI_TOUCH);
            CloseMobileSidebar();
        }

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

                case "deleted":
                    Ui.Toast.Brief(Ui.Lang["mainlayout.Toast.AccountDeleted"]);
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

        private async Task OnBackClick()
        {
            await Audio.PlaySfxAsync(AudioService.SFX_UI_TOUCH);
            Ui.SubHeader.Hide();
            Ui.Header.SetBackBtnToPushState();
        }
        private void ShowModal() => _ = _mainModal!.ShowAsync(Ui.Modal.Parameter!);
        private void HideModal() => _ = _mainModal!.HideAsync();
        private void Refresh() => InvokeAsync(StateHasChanged);
        private void ModalAction(ModalResult result) => Ui.Modal.SendResult(result);

        private async Task SetSoundEnabledAsync(bool enabled)
        {
            await Settings.SetSoundEnabledAsync(enabled);
        }
        private async Task Logout()
        {
            await User.LogoutAsync(false);
            Console.WriteLine("User logged out.");
        }
        private Task OpenHelpAsync() => OpenAuxiliaryWindowAsync(
            () => _helpNavigator?.ShowManualAsync() ?? Task.CompletedTask);

        private Task OpenProfileAsync() => OpenAuxiliaryWindowAsync(
            () => _profileNavigator?.ShowAsync() ?? Task.CompletedTask);

        private Task OpenSettingsAsync() => OpenAuxiliaryWindowAsync(
            () => _settingsNavigator?.ShowAsync() ?? Task.CompletedTask);

        private Task OpenCurrentHelpAsync() => OpenAuxiliaryWindowAsync(
            () => _helpNavigator?.ShowCurrentAsync() ?? Task.CompletedTask);

        private async Task OpenAuxiliaryWindowAsync(Func<Task> openAsync)
        {
            if (Ui.Modal.Parameter is not null)
                return;

            await CloseAuxiliaryWindowsAsync();
            await openAsync();
        }

        private Task CloseAuxiliaryWindowsAsync()
        {
            if (Ui.Modal.Parameter is not null)
            {
                Ui.Modal.SendResult(ModalResult.Close);
                return Task.CompletedTask;
            }


            return Task.WhenAll(
                _helpNavigator?.Close() ?? Task.CompletedTask,
                _settingsNavigator?.Close() ?? Task.CompletedTask,
                _profileNavigator?.Close() ?? Task.CompletedTask);
        }

        private Task OnRefreshRequired(ReqStates[] reqTypes) =>
            InitStatesAsync(reqTypes);

        private Task RefreshProfileAsync() =>
            InitStatesAsync(
                ReqStates.Home,
                ReqStates.Team);
    }
}

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
        private async Task InitStatesAsync(params ReqStates[] reqTypes)
        {

            var allViaCheckIn = reqTypes[0] == ReqStates.AllViaCheckIn;
            var allStates = reqTypes[0] is ReqStates.All or ReqStates.AllViaCheckIn;

            if (!await RestoreCheckInSessionAsync(allViaCheckIn))
                return;

            if (allStates)
                reqTypes = Enum.GetValues<ReqStates>()[2..];

            _appState.Culture = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;

            foreach (var reqType in reqTypes)
            {
                if (!await LoadStateAsync(reqType))
                    return;
            }

            if (allViaCheckIn)
                _isLoggedIn = true;

            await CompleteStateInitializationAsync(allStates);

            await InvokeAsync(StateHasChanged);
        }

        private async Task<bool> RestoreCheckInSessionAsync(
            bool allViaCheckIn)
        {
            if (!allViaCheckIn || await RestoreSessionAsync())
                return true;

            _isLoggedIn = false;
            await InvokeAsync(StateHasChanged);
            return false;
        }

        private async Task<bool> LoadStateAsync(ReqStates reqType)
        {
            switch (reqType)
            {
                case ReqStates.Home:
                    return await LoadHomeStateAsync();

                case ReqStates.Question:
                    return await LoadQuestionStateAsync();

                case ReqStates.Team:
                    return await LoadTeamStateAsync();

                case ReqStates.SoloGame:
                    return await LoadSoloGameStateAsync();

                case ReqStates.VsGame:
                    return await LoadVsGameStateAsync();

                case ReqStates.LocalSotrage:
                    await LoadLocalStorageStateAsync();
                    return true;

                default:
                    return true;
            }
        }

        private async Task<bool> LoadHomeStateAsync()
        {
            HState.Invalidate();
            await HState.EnsureLoadedAsync();
            if (!HState.IsLoaded)
                return false;

            _appState.Home = HState.Snapshot;
            UpdateHeadDisplay();
            return true;
        }

        private async Task<bool> LoadQuestionStateAsync()
        {
            QState.Invalidate();
            await QState.EnsureLoadedAsync();
            if (!QState.IsLoaded)
                return false;

            _appState.Question = QState.Snapshot;
            return true;
        }

        private async Task<bool> LoadTeamStateAsync()
        {
            TState.Invalidate();
            await TState.EnsureLoadedAsync();
            if (!TState.IsLoaded)
                return false;

            _appState.Team = TState.Snapshot;
            return true;
        }

        private async Task<bool> LoadSoloGameStateAsync()
        {
            SState.Invalidate();
            await SState.EnsureLoadedAsync();
            if (!SState.IsLoaded)
                return false;

            _appState.SoloGame = SState.Snapshot;
            return true;
        }

        private async Task<bool> LoadVsGameStateAsync()
        {
            VState.Invalidate();
            await VState.EnsureLoadedAsync();
            if (!VState.IsLoaded)
                return false;

            _appState.VsGame = VState.Snapshot;
            return true;
        }

        private async Task LoadLocalStorageStateAsync()
        {
            _appState.LocStoreStates.ChkBxNotShowDel =
                await LocalStorage.GetItemAsync<bool>(_localNotShowDel);
            _appState.LocStoreStates.ChkBxNotShowNew =
                await LocalStorage.GetItemAsync<bool>(_localNotShowNew);
            _appState.LocStoreStates.LastBboardChk =
                await LocalStorage.GetItemAsync<DateTime>(LOCAL_LAST_B_BOARD);
            _appState.LocStoreStates.SeenHelps =
                await LocalStorage.GetItemAsync<HashSet<int>>(
                    HelpCollection.SEEN_STORAGE_KEY) ?? [];
        }

        private async Task CompleteStateInitializationAsync(bool allStates)
        {
            if (!allStates)
                return;

            await Audio.PlayMusicAsync(MusicTrack.MenuMain);

            if (!SessionService.PendingSessionReplacementWarning)
                return;

            SessionService.PendingSessionReplacementWarning = false;
            Ui.Toast.Brief(Ui.Lang["mainlayout.Toast.Login.Replaced"]);
        }

        private async Task<bool> RestoreSessionAsync()
        {
            var sessionId = await SessionStorage.GetItemAsync<string>("SessionId");
            if (string.IsNullOrWhiteSpace(sessionId))
                return false;

            SessionService.SessionId = sessionId;

            return true;
        }

        private void UpdateHeadDisplay()
        {
            var home = _appState.Home;
            if (home?.UserMainData is null ||
                home.ExtendedInfo is null)
            {
                return;
            }

            var main = home.UserMainData;
            var level = RankNameTable.Data[main.RankEnum]
                .PublicLevel ?? string.Empty;

            Ui.HeadDisplay.SetMessages(
            [
                Ui.Lang["mainlayout.Text.TeamName"]
                    .FormatSafe(main.TeamName),
                Ui.Lang["mainlayout.Text.TeamLevel"]
                    .FormatSafe(level),
                Ui.Lang["mainlayout.Text.Xp"]
                    .FormatSafe(main.XP),
                Ui.Lang["mainlayout.Text.NextLevelXp"]
                    .FormatSafe(home.ExtendedInfo.NextXp),
                Ui.Lang["mainlayout.Text.Credit"]
                    .FormatSafe(main.Credit),
                Ui.Lang["mainlayout.Text.Voucher"]
                    .FormatSafe(main.Voucher)
            ]);
        }

        private async Task<string> InitCultureAsync()
        {
            var culture = await LocalStorage.GetItemAsync<string>("userLang");
            if (string.IsNullOrWhiteSpace(culture))
            {
                culture = "hu-HU";
                await LocalStorage.SetItemAsync("userLang", culture);
            }
            CultureInfo.DefaultThreadCurrentCulture = new CultureInfo(culture);
            CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo(culture);

            return CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
        }
    }
}

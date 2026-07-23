using Blazored.LocalStorage;
using Blazored.SessionStorage;
using KvizCommando.Client.Helpers;
using KvizCommando.Client.Pages.Shared.Modal;
using KvizCommando.Client.Pages.Shared.Modal.Features;
using KvizCommando.Client.Services.Audio;
using KvizCommando.Client.Services.ClientCache;
using KvizCommando.Client.Services.Visual.UiService;
using KvizCommando.Client.Utilities;
using KvizCommando.Shared.Models.Dtos;
using Microsoft.AspNetCore.Components;
using System.Globalization;

namespace KvizCommando.Client.Layout
{
    public partial class MainLayout : KcLayoutComponentBase, IDisposable
    {
        [Inject] private ILocalStorageService LocalStorage { get; set; } = default!;
        [Inject] private ISessionStorageService SessionStorage { get; set; } = default!;
        [Inject] private IHomeState HState { get; set; } = default!;
        [Inject] private IQuestionState QState { get; set; } = default!;
        [Inject] private ITeamState TState { get; set; } = default!;
        [Inject] private ISoloState SState { get; set; } = default!;
        [Inject] private AudioService Audio { get; set; } = default!;
        [Inject] private SessionService SessionService { get; set; } = default!;

        private static readonly string _localNotShowNew = ModalConst.LOCAL_NOT_SHOW_NEW;
        private static readonly string _localNotShowDel = ModalConst.LOCAL_NOT_SHOW_DEL;
        private const string LOCAL_LAST_B_BOARD = "B.B";

        private readonly AppState _appState = new();

        private KcModal? _mainModal;

        private string _culture = "hu";
        private bool _isReady = false;
        private bool _isLoggedIn = false;
        private bool _isMusicOn;
        private bool _isBckBtnEna = false;
        private string _currentTitle = string.Empty;
        private bool _hasSidebarCollapsed = false;

        private string Greetings => _isLoggedIn
            ? Ui.Lang["mainlayout.Text.Greetings"].FormatSafe(RankNameLocalizer.GetName(_appState.Home!.UserMainData.RankEnum, _culture))
            : string.Empty;

        private void ToggleSidebar() => _hasSidebarCollapsed = (!_hasSidebarCollapsed && _isLoggedIn);
        private bool BackNavigationEna => (_hasSidebarCollapsed && Ui.Header.PageIndex != 0) || _isBckBtnEna;
        private bool SideBarStatus => _hasSidebarCollapsed || !_isLoggedIn;
        private HomeScreen Hs => _isLoggedIn && !(Ui.Header.PageIndex >= 420 && Ui.Header.PageIndex <= 470) ? _appState.Home!.HomeScreen : new HomeScreen() { };
        protected override async Task OnInitializedAsync()
        {
            _isReady = false;

            Console.WriteLine($"[{this}] has been started");

            _culture = await InitCultureAsync();

            await Ui.Lang.LoadModuleAsync(_culture, "common");  // szükséges
            await Ui.Lang.LoadModuleAsync(_culture, "mainlayout");  // szükséges
            await Ui.Lang.LoadModuleAsync(_culture, "home");

            var sessionId = await SessionStorage.GetItemAsync<string>("SessionId");

            if (!string.IsNullOrWhiteSpace(sessionId))
            {
                SessionService.SessionId = sessionId;
                await InitStatesAsync(0);
                _isLoggedIn = true;
            }
            else
            {
                _isLoggedIn = false;
                var page = Ui.Nav.ToBaseRelativePath(Ui.Nav.Uri);

                if (!string.IsNullOrEmpty(page))
                {
                    Ui.Nav.NavigateTo("/", true);
                    return;
                }

            }

            if (Audio.EnteredNormal)
            {
                Console.WriteLine("Playing music because EnteredNormal is true.");
                await Audio.PlayMusicAsync("Menu02.webm");
                _isMusicOn = true;
            }
            else
            {
                Console.WriteLine("Not playing music because EnteredNormal is false.");
            }

            _isReady = true;
        }
        protected override void OnInitialized()
        {
            Ui.Header.OnTitleChanged += UpdateTitle;
            Ui.Header.OnBackBtnEnaChanged += UpdateBackBtnEna;
            Ui.Modal.OnModalShow += ShowModal;
            Ui.Modal.OnModalHide += HideModal;
            Ui.ReloadRequested += OnRefreshRequired;
            Ui.SubHeader.OnButtonsChanged += Refresh;
        }

        private void UpdateTitle()
        {
            _currentTitle = Ui.Header.Title;
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
        private void ModalAction(ModalResult result)
        {
            Ui.Modal.SendResult(result);
        }
        private async Task OnMusicClick()
        {
            _isMusicOn = !_isMusicOn;
            if (Audio.EnteredNormal)
            {
                await Audio.SetMusicEnabledAsync(_isMusicOn);
            }
            else
            {
                Audio.EnteredNormal = true;
                await Audio.PlayMusicAsync("Menu02.webm");
            }

        }
        private async Task Logout()
        {
            await User.LogoutAsync(false);
            Console.WriteLine("User logged out.");
        }
        private async Task OnRefreshRequired(ReqStates reqType)
        {
            await InitStatesAsync(reqType);
        }
        private async Task InitStatesAsync(ReqStates state)
        {
            _appState.Culture = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
            if (state == ReqStates.All || state == ReqStates.Home)
            {
                HState.Invalidate();
                await HState.EnsureLoadedAsync();
                _appState.Home = HState.Snapshot;
            }
            if (state == ReqStates.All || state == ReqStates.Question)
            {
                QState.Invalidate();
                await QState.EnsureLoadedAsync();
                _appState.Question = QState.Snapshot;
            }
            if (state == ReqStates.All || state == ReqStates.Team)
            {
                TState.Invalidate();
                await TState.EnsureLoadedAsync();
                _appState.Team = TState.Snapshot;
            }
            if (state == ReqStates.All || state == ReqStates.SoloGame)
            {
                SState.Invalidate();
                await SState.EnsureLoadedAsync();
                _appState.SoloGame = SState.Snapshot;
            }
            if (state == ReqStates.All || state == ReqStates.LocalSotrage)
            {
                _appState.LocStoreStates.ChkBxNotShowDel = await LocalStorage.GetItemAsync<bool>(_localNotShowDel);
                _appState.LocStoreStates.ChkBxNotShowNew = await LocalStorage.GetItemAsync<bool>(_localNotShowNew);
                _appState.LocStoreStates.LastBboardChk = await LocalStorage.GetItemAsync<DateTime>(LOCAL_LAST_B_BOARD);
            }
            await InvokeAsync(StateHasChanged);
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
        public void Dispose()
        {
            Ui.Header.OnTitleChanged -= UpdateTitle;
            Ui.Header.OnBackBtnEnaChanged -= UpdateBackBtnEna; // <-- a helyes handlerre iratkozunk le
            Ui.Modal.OnModalShow -= ShowModal;
            Ui.Modal.OnModalHide -= HideModal;
            Ui.ReloadRequested -= OnRefreshRequired;
            Ui.SubHeader.OnButtonsChanged -= Refresh;
            _mainModal?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
    public enum ReqStates
    {
        All = 0,
        Home,
        Question,
        Team,
        SoloGame,
        LocalSotrage
    }
}


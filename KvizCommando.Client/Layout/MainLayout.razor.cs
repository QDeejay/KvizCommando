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
    /// <summary>
    /// Az alkalmazás fő elrendezését és közös kliensállapotát kezeli.
    /// </summary>
    public partial class MainLayout : KcLayoutComponentBase, IDisposable
    {
[Inject] private ILocalStorageService LocalStorage { get; set; } = default!;
        [Inject] private ISessionStorageService SessionStorage { get; set; } = default!;
        [Inject] private IHomeState HState { get; set; } = default!;
        [Inject] private IQuestionState QState { get; set; } = default!;
        [Inject] private ITeamState TState { get; set; } = default!;
        [Inject] private ISoloState SState { get; set; } = default!;
        [Inject] private IVsState VState { get; set; } = default!;
        [Inject] private AudioService Audio { get; set; } = default!;
        [Inject] private SessionService SessionService { get; set; } = default!;

        private static readonly string _localNotShowNew = ModalConst.LOCAL_NOT_SHOW_NEW;
        private static readonly string _localNotShowDel = ModalConst.LOCAL_NOT_SHOW_DEL;
        private const string LOCAL_LAST_B_BOARD = "B.B";

        private readonly AppState _appState = new();

        private HelpNavigator? _helpNavigator;
        private KcModal? _mainModal;

        private string _culture = "hu";
        private bool _isReady = false;
        private bool _isLoggedIn = false;
        private bool _isBckBtnEna = false;
        private string _currentTitle = string.Empty;
        private bool _isDesktopNavOpen = true;
        private bool _isMobileNavOpen;

        private string Greetings => _isLoggedIn
            ? Ui.Lang["mainlayout.Text.Greetings"].FormatSafe(RankNameLocalizer.GetName(_appState.Home!.UserMainData.RankEnum, _culture))
            : string.Empty;

        private bool IsFullScreenGame =>
            Ui.Header.PageIndex is >= (int)SgameBoxKeyRoot.Category and <= (int)SgameBoxKeyRoot.Category + SoloBoxSpecs.CATEGORY_BOX_COUNT ||
            Ui.Header.PageIndex is >= (int)SgameBoxKeyRoot.Orientation and <= (int)SgameBoxKeyRoot.Orientation + SoloBoxSpecs.ORIENTATION_BOX_COUNT ||
            Ui.Header.PageIndex is > (int)VsBoxKeyRanked.Classification and <= (int)VsBoxKeyRanked.Classification + VsGameBoxSpecs.CLASSIFICATION_BOX_COUNT;
        private bool CanToggleSidebar => _isLoggedIn && Hs.NavBarEnable;
        private bool BackNavigationEna => (!_isMobileNavOpen && Ui.Header.PageIndex != 0) || _isBckBtnEna;
        private HomeScreen Hs =>
            _isLoggedIn && !IsFullScreenGame
                ? _appState.Home!.HomeScreen
                : new();
        private bool IsRenderReady =>
                _isReady && (!_isLoggedIn || _appState.Home is not null);

        protected override async Task OnInitializedAsync()
        {
            _isReady = false;

            Console.WriteLine($"[{this}] has been started");

            _culture = await InitCultureAsync();

            await Ui.Lang.LoadModuleAsync(_culture, "common");  // szükséges
            await Ui.Lang.LoadModuleAsync(_culture, "mainlayout");  // szükséges
            await Ui.Lang.LoadModuleAsync(_culture, "home");
            var uri = Ui.Nav.ToAbsoluteUri(Ui.Nav.Uri);
            var loggedOut = ShowLogoutToast(uri);

            if (await RestoreSessionAsync() && !loggedOut)
            {
                await InitStatesAsync(ReqStates.All);
                _isLoggedIn = true;
            }
            else
            {
                _isLoggedIn = false;
                var page = Ui.Nav.ToBaseRelativePath(
                    uri.GetLeftPart(UriPartial.Path));

                if (!string.IsNullOrEmpty(page))
                {
                    Ui.Nav.NavigateTo("/", true);
                    return;
                }

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

        /// <inheritdoc />
        public void Dispose()
        {
            Ui.Header.OnTitleChanged -= UpdateTitle;
            Ui.Header.OnBackBtnEnaChanged -= UpdateBackBtnEna; // <-- a helyes handlerre iratkozunk le
            Ui.Modal.OnModalShow -= ShowModal;
            Ui.Modal.OnModalHide -= HideModal;
            Ui.ReloadRequested -= OnRefreshRequired;
            Ui.SubHeader.OnButtonsChanged -= Refresh;
            GC.SuppressFinalize(this);
            //_mainModal?.Dispose();
        }
    }
}

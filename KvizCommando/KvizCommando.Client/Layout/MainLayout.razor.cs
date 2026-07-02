using Blazored.LocalStorage;
using Blazored.SessionStorage;
using CsvHelper.Configuration.Attributes;
using KvizCommando.Client.Data;
using KvizCommando.Client.Features.Modal;
using KvizCommando.Client.Helpers;
using KvizCommando.Client.Models.ViewModels;
using KvizCommando.Client.Pages.Shared;
using KvizCommando.Client.Services;
using KvizCommando.Client.Services.Audio;
using KvizCommando.Client.Services.ClientCache;
using KvizCommando.Client.Services.User;
using KvizCommando.Client.Services.Visual.UiService;
using KvizCommando.Client.Utilities;
using KvizCommando.Shared.Models.Dtos;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using System.Globalization;
using System.Reflection.Emit;
using System.Reflection.PortableExecutable;
using System.Security.AccessControl;

namespace KvizCommando.Client.Layout
{
    public partial class MainLayout : KcLayoutComponentBase, IDisposable
    {
        [Inject] private ILocalStorageService LocalStorage { get; set; } = default!;
        [Inject] private ISessionStorageService SessionStorage { get; set; } = default!;
        [Inject] private SessionService SessionService { get; set; } = default!;
        [Inject] private IHomeState HState { get; set; } = default!;
        [Inject] private IQuestionState QState { get; set; } = default!;
        [Inject] private ITeamState TState { get; set; } = default!;
        [Inject] private ISoloState SState { get; set; } = default!;
        [Inject] private AudioService Audio { get; set; } = default!;

        private const string LOCAL_NOT_SHOW_NEW = "notShowNew";
        private const string LOCAL_NOT_SHOW_DEL = "notShowDel";
        private const string LOCAL_LAST_B_BOARD = "B.B";

        private readonly AppState _appState = new();

        private ModalBoxVm _modalPar = new();
        private KcModal? _mainModal;
  

        private string _culture = "hu";
        private bool _isReady = false;
        private bool _isLoggedIn = false;
        private bool _isMusicOn;
        private bool _isBckBtnEna = false;
        private string? _currentTitle = string.Empty;
        private bool _hasSidebarCollapsed = false;
        private string? _Greetings = string.Empty;
        private int _NavigateTo = 0;

        private void ToggleSidebar() => _hasSidebarCollapsed = (!_hasSidebarCollapsed && _isLoggedIn);
        private bool BackNavigationEna => (_hasSidebarCollapsed && Ui.Header.PageIndex!=0) || _isBckBtnEna;
        private HomeScreen Hs => _isLoggedIn ? _appState.Home!.HomeScreen : new HomeScreen() {  };
        protected override async Task OnInitializedAsync()
        {
            _isReady = false;
          
            Console.WriteLine($"[{this}] has been started");
            var culture = await LocalStorage.GetItemAsync<string>("userLang");
            if (string.IsNullOrWhiteSpace(culture))
            {
                culture = "hu-HU";
                await LocalStorage.SetItemAsync("userLang", culture);
            }
            CultureInfo.DefaultThreadCurrentCulture = new CultureInfo(culture);
            CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo(culture);
            _culture = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
            
            await Ui.Lang.LoadModuleAsync(_culture, "common");  // szükséges
            await Ui.Lang.LoadModuleAsync(_culture, "mainlayout");  // szükséges
            await Ui.Lang.LoadModuleAsync(_culture, "home");
            var sessionId = await SessionStorage.GetItemAsync<string>("SessionId");
            if (!string.IsNullOrWhiteSpace(sessionId))
            {
                SessionService.SessionId = sessionId;
                await InitStatesAsync(0);
                //await HomeState.EnsureLoadedAsync();
                _isLoggedIn = true;
               
            }
            else { _isLoggedIn = false; }
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
        private void TestShow()
        {
          //  _ =  _mainModal!.ShowAsync();
        }
        protected override void OnInitialized()
        {
            Ui.Header.OnTitleChanged += UpdateTitle;
            Ui.Header.OnRankChanged += UpdateRank;
            Ui.Header.OnBackBtnEnaChanged += UpdateBackBtnEna;
            Ui.Modal.OnModalShow += ShowModal;
            Ui.Modal.OnModalHide += HideModal;

            Ui.ReloadRequested += OnRefreshRequired;

        }
        private void UpdateTitle()
        {
            _currentTitle = Ui.Header.Title;
            InvokeAsync(StateHasChanged);
        }
        private void UpdateRank() 
        {
            var rankName = RankNameLocalizer.GetName(Ui.Header.Rank, _culture);
            _Greetings = Ui.Lang["mainlayout.Text.Greetings"].FormatSafe(rankName);
            InvokeAsync(StateHasChanged);
        }
        private void UpdateBackBtnEna() 
        {
            _isBckBtnEna = Ui.Header.BackEna;
            InvokeAsync(StateHasChanged);
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
        private void OnBackClick()
        {
            Ui.Header.SetBackBtnToPushState();
            Console.WriteLine("Back button clicked.");
        }

        private void ShowModal()
        {
            _modalPar = Ui.Modal.Parameter!;
            _ = _mainModal!.ShowAsync(_modalPar);
            //await InvokeAsync(async () =>
            //{});
        }

        private void HideModal()
        {
            _ = _mainModal!.HideAsync();
        }

        private void ModalAction(ModalResult result)
        {
            Ui.Modal.SendResult(result);
        }


        public void Dispose()
        {
            Ui.Header.OnTitleChanged -= UpdateTitle;
            Ui.Header.OnRankChanged -= UpdateRank;
            Ui.Header.OnBackBtnEnaChanged -= UpdateBackBtnEna; // <-- a helyes handlerre iratkozunk le
            Ui.Modal.OnModalShow -= ShowModal;
            Ui.Modal.OnModalHide -= HideModal;
            Ui.ReloadRequested -= OnRefreshRequired;
            GC.SuppressFinalize(this);


        }
        public async Task Logout()
        {
            await User.LogoutAsync(false);
            Console.WriteLine("User logged out.");
        }
        private async Task OnRefreshRequired()
        {
            await InitStatesAsync(ReqStates.All);
        }
        public async Task InitStatesAsync(ReqStates state)
        {
            _appState.Culture = CultureInfo.CurrentCulture.TwoLetterISOLanguageName; 
            if (state == ReqStates.All || state == ReqStates.Home)
            {
                await HState.EnsureLoadedAsync();
                _appState.Home = HState.Snapshot;
            }
            if (state == ReqStates.All || state == ReqStates.Question)
            {
                await QState.EnsureLoadedAsync();
                _appState.Question = QState.Snapshot;
            }
            if (state == ReqStates.All || state == ReqStates.Team)
            {
                await TState.EnsureLoadedAsync();
                _appState.Team = TState.Snapshot;
            }
            if (state == ReqStates.All || state == ReqStates.SoloGame)
            {
                await SState.EnsureLoadedAsync();
                _appState.SoloGame = SState.Snapshot;
            }
            if (state == ReqStates.All || state == ReqStates.LocalSotrage)
            {
                _appState.LocStoreStates.ChkBxNotShowDel = await LocalStorage.GetItemAsync<bool>(LOCAL_NOT_SHOW_DEL);
                _appState.LocStoreStates.ChkBxNotShowNew = await LocalStorage.GetItemAsync<bool>(LOCAL_NOT_SHOW_NEW);
                _appState.LocStoreStates.LastBboardChk = await LocalStorage.GetItemAsync<DateTime>(LOCAL_LAST_B_BOARD);
            }
            await InvokeAsync(StateHasChanged);
        }
    }
    public enum ReqStates 
    {
        All=0,
        Home,
        Question,
        Team,
        SoloGame,
        LocalSotrage
    }
}
/*
 
        //[Inject] private ILanguageService Lang { get; set; } = default!;
        //[Inject] private NavigationManager Nav { get; set; } = default!;
        //[Inject] private PageHeaderService Header { get; set; } = default!;
        //[Inject] private ModalService Modal { get; set; } = default!;   
        //[Inject] private IDisplayMessageState DisplayState { get; set; } = default!;

        //[Inject] private IUserService User { get; set; } = default!;
        private void HeadDisplayUpdate()
        {

            _ = InvokeAsync(StateHasChanged);
        }
   protected override void OnInitialized()
        {
           // _currentTitle = PageTitle.Title;
           // var rankName = PageTitle.Rank>=0 ? RankNameLocalizer.GetName(PageTitle.Rank, culture) : "";
           // _Greetings = Lang["mainlayout.Text.Greetings"].FormatSafe(rankName);
           // 

        }
 
 */
using Blazored.LocalStorage;
using Blazored.SessionStorage;
using CsvHelper.Configuration.Attributes;
using KvizCommando.Client.Data;
using KvizCommando.Client.Helpers;
using KvizCommando.Client.Services;
using KvizCommando.Client.Services.Audio;
using KvizCommando.Client.Services.ClientCache;
using KvizCommando.Client.Services.Language;
using KvizCommando.Client.Services.User;
using KvizCommando.Client.Services.Visual;
using KvizCommando.Shared.Models.Dtos;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Globalization;
using System.Reflection.Emit;

namespace KvizCommando.Client.Layout
{
    public partial class MainLayout : LayoutComponentBase, IDisposable
    {
        [Inject] private ILanguageService Lang { get; set; } = default!;
        [Inject] private ILocalStorageService LocalStorage { get; set; } = default!;
        [Inject] private ISessionStorageService SessionStorage { get; set; } = default!;
        [Inject] private NavigationManager Nav { get; set; } = default!;
        [Inject] private SessionService SessionService { get; set; } = default!;
        [Inject] private PageTitleService PageTitle { get; set; } = default!;
        [Inject] private IDisplayMessageState DisplayState { get; set; } = default!;
        [Inject] private IHomeState HomeState { get; set; } = default!;
        [Inject] private IUserService UserService { get; set; } = default!;
        [Inject] private AudioService Audio { get; set; } = default!;

        private string _culture = "hu";
        private bool _isReady = false;
        private bool _loggedIn = false;
        private bool _isMusicOn;
        private string? _currentTitle = string.Empty;
        private bool _sidebarCollapsed = false;
        private string? _Greetings = string.Empty;
        private int NavigateTo = 0;

        private void ToggleSidebar() => _sidebarCollapsed = (!_sidebarCollapsed && _loggedIn);
        private bool _backNavigationEna => _sidebarCollapsed ? PageTitle.NavPage > 0 : PageTitle.NavPage > 99;
        private HomeScreen Hs => _loggedIn ? HomeState.HomeScreen! : new HomeScreen() {  };
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
            await Lang.LoadModuleAsync(_culture, "common");  // szükséges
            await Lang.LoadModuleAsync(_culture, "mainlayout");  // szükséges
            await Lang.LoadModuleAsync(_culture, "home");

            var sessionId = await SessionStorage.GetItemAsync<string>("SessionId");
            if (!string.IsNullOrWhiteSpace(sessionId))
            {
                SessionService.SessionId = sessionId;
                await HomeState.EnsureLoadedAsync();
                _loggedIn = true;
            }
            else { _loggedIn = false; }
             

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

            PageTitle.OnTitleChanged += UpdateTitle;
            _isReady = true;
        }
        protected override void OnInitialized()
        {
           // _currentTitle = PageTitle.Title;
           // var rankName = PageTitle.Rank>=0 ? RankNameLocalizer.GetName(PageTitle.Rank, culture) : "";
           // _Greetings = Lang["mainlayout.Text.Greetings"].FormatSafe(rankName);
           // 

        }
        private void UpdateTitle()
        {
            _currentTitle = PageTitle.Title;
            var rankName = PageTitle.Rank >= 0 ? RankNameLocalizer.GetName(PageTitle.Rank, _culture) : "";
            _Greetings = Lang["mainlayout.Text.Greetings"].FormatSafe(rankName);
            _ = InvokeAsync(StateHasChanged);
        }
        private void HeadDisplayUpdate()
        {

            _ = InvokeAsync(StateHasChanged);
        }
        protected async Task OnMusicClick()
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
        protected void OnBackClick()
        {
            OnSelect(PageTitle!.PrevPage);
            Console.WriteLine("Back button clicked.");
        }
        private void OnSelect(int selected)
        {
            NavigateTo = selected;

        }
        public void Dispose()
        {
            PageTitle.OnTitleChanged -= UpdateTitle; // <-- a helyes handlerre iratkozunk le
            GC.SuppressFinalize(this);
        }
        public async void Logout()
        {
           
            await Task.Delay(1);
            await UserService.LogoutAsync(false);
            Console.WriteLine("User logged out.");
        }
    }
}

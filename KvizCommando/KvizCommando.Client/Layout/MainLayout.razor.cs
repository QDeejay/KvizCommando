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
        [Inject] public PageTitleService PageTitle { get; set; } = default!;
        //[Inject] public IDisplayMessageState DisplayState { get; set; } = default!;
        [Inject] private IHomeState HomeState { get; set; } = default!;
        [Inject] private IUserService UserService { get; set; } = default!;
        [Inject] private ILoadingService Loader { get; set; } = default!;
        [Inject] private AudioService Audio { get; set; } = default!;

        private string culture = "hu";
        private bool _isReady = false;
        private bool _isMusicOn;
        private string? _currentTitle = string.Empty;
        private bool sidebarCollapsed = false;
        private string? _Greetings = string.Empty;

        private void ToggleSidebar() => sidebarCollapsed = !sidebarCollapsed;
        private bool _backNavigationEna => sidebarCollapsed ? PageTitle.NavPage > 0 : PageTitle.NavPage > 99;
        private HomeScreen Hs => HomeState.HomeScreen!;
        protected override async Task OnInitializedAsync()

        {
            await Loader.Show();
            Console.WriteLine($"[{this}] has been started");
            _isReady = false;
            culture = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;

            await Lang.LoadModuleAsync(culture, "mainlayout");  // szükséges
            await Lang.LoadModuleAsync(culture, "home");
            await HomeState.EnsureLoadedAsync();
            //await QuestionState.EnsureLoadedAsync();

            

            //var rankName = RankNameLocalizer.GetName(level, culture);
            //_Greetings = Lang["mainlayout.Text.Greetings"].FormatSafe(rankName);

            // await JS.InvokeVoidAsync("appHeader.setUser", true, HomeState.UserMainData.UserName);
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
            _currentTitle = PageTitle.Title;
            var rankName = PageTitle.Rank>=0 ? RankNameLocalizer.GetName(PageTitle.Rank, culture) : "";
            _Greetings = Lang["mainlayout.Text.Greetings"].FormatSafe(rankName);
            PageTitle.OnTitleChanged += UpdateTitle;

        }
        private void UpdateTitle()
        {
            _currentTitle = PageTitle.Title;
            var rankName = PageTitle.Rank >= 0 ? RankNameLocalizer.GetName(PageTitle.Rank, culture) : "";
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
            Console.WriteLine("Back button clicked.");
        }
        private async Task OnSelect(int selected)
        {
            await Task.Delay(100);
        }
        public void Dispose()
        {
            PageTitle.OnTitleChanged -= UpdateTitle; // <-- a helyes handlerre iratkozunk le
            GC.SuppressFinalize(this);
        }
        public async void Logout()
        {
            await Loader.Show();
            await Task.Delay(501);
            await UserService.LogoutAsync(false);
            Console.WriteLine("User logged out.");
        }
    }
}

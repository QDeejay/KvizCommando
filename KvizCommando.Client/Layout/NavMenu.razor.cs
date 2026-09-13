using KvizCommando.Client.Services.Audio;
using KvizCommando.Localization.MainLayout;
using KvizCommando.Shared.Models.Dtos;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using System.Reflection;

namespace KvizCommando.Client.Layout
{
    public partial class NavMenu
    {
        [Inject] private AudioService Audio { get; set; } = default!;
        [Inject] private IStringLocalizer<NavMenuResource> Lang { get; set; } = default!;
        [Parameter] public HomeScreen Hs { get; set; } = default!;
        [Parameter] public EventCallback OnClose { get; set; }


        private bool _isReady;
        private string[] _btnNavClass = new string[16];
        private const string BTN_NAV_CLASS_DEF = "navigation-button";
        private static readonly string _deployVersion = ResolveDeployVersion();


        protected override void OnParametersSet()
        {
            if (Hs != null)
            {
                _btnNavClass[0] = BTN_NAV_CLASS_DEF + (!Hs.NavBarEnable ? " disabled" : "");
                _btnNavClass[1] = BTN_NAV_CLASS_DEF + (!Hs.Team.Enable ? " disabled" : "");     // Team
                _btnNavClass[2] = BTN_NAV_CLASS_DEF + (!Hs.Question.Enable ? " disabled" : ""); // Question
                _btnNavClass[3] = BTN_NAV_CLASS_DEF + (!Hs.SoloGame.Enable ? " disabled" : "");// Game
                _btnNavClass[4] = BTN_NAV_CLASS_DEF + (!Hs.VsGame.Enable ? " disabled" : "");// VsGame
                _btnNavClass[5] = BTN_NAV_CLASS_DEF + (!Hs.Shop.Enable ? " disabled" : ""); // Shop
                _btnNavClass[6] = BTN_NAV_CLASS_DEF + (!Hs.Ranking.Enable ? " disabled" : ""); // Rankings
                _btnNavClass[7] = BTN_NAV_CLASS_DEF + (!Hs.Statistic.Enable ? " disabled" : "");  // Statistic
                _btnNavClass[8] = BTN_NAV_CLASS_DEF + (!Hs.Events.Enable ? " disabled" : "");   // Events
                _btnNavClass[9] = BTN_NAV_CLASS_DEF + (!Hs.Community.Enable ? " disabled" : ""); // Community
                _btnNavClass[10] = BTN_NAV_CLASS_DEF + (!Hs.Messages.Enable ? " disabled" : "");// Messages
                _btnNavClass[11] = BTN_NAV_CLASS_DEF;                                        // Settings allways on
                _btnNavClass[15] = BTN_NAV_CLASS_DEF;                                        // Exit allways on
                _isReady = true;
            }
        }

        private async Task CloseAsync()
        {
            await Audio.PlaySfxAsync(AudioService.SFX_UI_TOUCH);
            await OnClose.InvokeAsync();
        }

        private async Task HandleNavigationAsync(bool isEnabled)
        {
            if (!isEnabled)
                return;

            await CloseAsync();
        }

        private static string ResolveDeployVersion()
        {
            string? version = typeof(NavMenu).Assembly
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                ?.InformationalVersion;

            return version?.StartsWith("v", StringComparison.Ordinal) == true
                ? version
                : "DEV";
        }
    }
}

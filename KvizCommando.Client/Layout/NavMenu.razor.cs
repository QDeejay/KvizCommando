using KvizCommando.Client.Services.Visual.UiService.Language;
using KvizCommando.Shared.Models.Dtos;
using Microsoft.AspNetCore.Components;
using KvizCommando.Client.Services.Audio;

namespace KvizCommando.Client.Layout
{
    public partial class NavMenu
    {
        [Inject] private AudioService Audio { get; set; } = default!;
        [Inject] private ILanguageService Lang { get; set; } = default!;
        [Parameter] public HomeScreen Hs { get; set; } = default!;
        [Parameter] public EventCallback OnClose { get; set; }


        private bool _isReady;
        private string[] btnNavClass = new string[16];
        private const string BTN_NAV_CLASS_DEF = "navigation-button";


        protected override void OnParametersSet()
        {
            if (Hs != null)
            {
                btnNavClass[0] = BTN_NAV_CLASS_DEF + (!Hs.NavBarEnable ? " disabled" : "");
                btnNavClass[1] = BTN_NAV_CLASS_DEF + (!Hs.Team.Enable ? " disabled" : "");     // Team
                btnNavClass[2] = BTN_NAV_CLASS_DEF + (!Hs.Question.Enable ? " disabled" : ""); // Question
                btnNavClass[3] = BTN_NAV_CLASS_DEF + (!Hs.SoloGame.Enable ? " disabled" : "");// Game
                btnNavClass[4] = BTN_NAV_CLASS_DEF + (!Hs.VsGame.Enable ? " disabled" : "");// VsGame
                btnNavClass[5] = BTN_NAV_CLASS_DEF + (!Hs.Shop.Enable ? " disabled" : ""); // Shop
                btnNavClass[6] = BTN_NAV_CLASS_DEF + (!Hs.Ranking.Enable ? " disabled" : ""); // Rankings
                btnNavClass[7] = BTN_NAV_CLASS_DEF + (!Hs.Statistic.Enable ? " disabled" : "");  // Statistic
                btnNavClass[8] = BTN_NAV_CLASS_DEF + (!Hs.Events.Enable ? " disabled" : "");   // Events
                btnNavClass[9] = BTN_NAV_CLASS_DEF + (!Hs.Community.Enable ? " disabled" : ""); // Community
                btnNavClass[10] = BTN_NAV_CLASS_DEF + (!Hs.Messages.Enable ? " disabled" : "");// Messages
                btnNavClass[11] = BTN_NAV_CLASS_DEF;                                        // Settings allways on
                btnNavClass[15] = BTN_NAV_CLASS_DEF;                                        // Exit allways on
                _isReady = true;
            }
        }

        private async Task CloseAsync()
        {
            await Audio.PlaySfxAsync(AudioService.SFX_UI_TOUCH);
            await OnClose.InvokeAsync();
        }
    }
}

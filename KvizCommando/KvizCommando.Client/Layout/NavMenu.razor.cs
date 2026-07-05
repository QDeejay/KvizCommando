using KvizCommando.Client.Pages.Solo;
using KvizCommando.Client.Services.ClientCache;
using KvizCommando.Client.Services.Visual.UiService.Language;
using KvizCommando.Shared.Models.Dtos;
using Microsoft.AspNetCore.Components;
using System.Globalization;

namespace KvizCommando.Client.Layout
{
    public partial class NavMenu
    {
        [Inject] private ILanguageService Lang { get; set; } = default!;
        [Parameter] public HomeScreen Hs { get; set; } = default!;
        [Parameter] public bool IsCollapsed { get; set; }
        [Parameter] public EventCallback OnToggleSidebar { get; set; }


        private bool _isReady ;
        private string[] btnNavClass = new string[16];
        private const string btnNavClassDef = "navigation-button";
        private string culture = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
        protected override void OnParametersSet()
        {
            if (Hs != null)
            {
                btnNavClass[0] = btnNavClassDef; // Home Allways on
                btnNavClass[1] = btnNavClassDef + (!Hs.Team.Enable ? " disabled" : "");     // Team 
                btnNavClass[2] = btnNavClassDef + (!Hs.Question.Enable ? " disabled" : ""); // Question
                btnNavClass[3] = btnNavClassDef + (!Hs.SoloGame.Enable ? " disabled" : "");// Game
                btnNavClass[4] = btnNavClassDef + (!Hs.VsGame.Enable ? " disabled" : "");// VsGame
                btnNavClass[5] = btnNavClassDef + (!Hs.Shop.Enable ? " disabled" : ""); // Shop
                btnNavClass[6] = btnNavClassDef + (!Hs.Statistic.Enable ? " disabled" : "");  // Statistic
                btnNavClass[7] = btnNavClassDef + (!Hs.Events.Enable ? " disabled" : "");   // Events
                btnNavClass[8] = btnNavClassDef + (!Hs.Messages.Enable ? " disabled" : "");// Messages
                btnNavClass[9] = btnNavClassDef + (!Hs.Community.Enable ? " disabled" : ""); // Community
                btnNavClass[10] = btnNavClassDef;                                        // Settings allways on
                btnNavClass[15] = btnNavClassDef;                                        // Exit allways on
                _isReady = true;
            }
        }

        protected override async Task OnInitializedAsync()
        {
            await Task.Delay(1);
        }
    }
}


using KvizCommando.Client.Services.Visual;
using KvizCommando.Client.Services.Visual.UiService.Language;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;


namespace KvizCommando.Client.Pages.Shared
{
    public partial class LanguageSelector : IDisposable
    {
        
        [Inject] private ILanguageService Lang { get; set; } = default!;

        private LanguageConfirmBase? ConfirmModal;
        protected override void OnInitialized()
        {
          //  Ui.Changed += OnUiChanged;
        }

        //private void OnUiChanged() => InvokeAsync(StateHasChanged);
        private void HuClick() => ShowConfirm("hu");
        private void EnClick() => ShowConfirm("en");
        private void ShowConfirm(string lang)
        {
            if (CultureInfo.CurrentCulture.TwoLetterISOLanguageName != lang)
            {
                ConfirmModal?.ShowForLanguage(
                        lang,
                        Lang[$"common.Modal.Language.Title.{lang}"],
                        Lang[$"common.Modal.Language.Content.{lang}"],
                        Lang[$"common.Modal.Language.Restart.{lang}"],
                        Lang["common.Modal.Language.Keep"]
                    );
            }
        }


        public void Dispose() 
        {
            //Ui.Changed -= OnUiChanged;
            GC.SuppressFinalize(this);
        }
    }
}

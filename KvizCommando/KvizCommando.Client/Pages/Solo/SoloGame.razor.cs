using KvizCommando.Client.Features.Sologame;
using KvizCommando.Client.Models.ViewModels;
using KvizCommando.Client.Services.ClientCache;
using KvizCommando.Client.Services.Dto;
using KvizCommando.Client.Services.Language;
using KvizCommando.Client.Services.Visual;
using KvizCommando.Shared.Models.Dtos;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;


namespace KvizCommando.Client.Pages.Solo
{
    public partial class SoloGame : ComponentBase
    {
        [Inject] protected PageTitleService PageTitle { get; set; } = default!;
       
        [Inject] protected ILanguageService Lang { get; set; } = default!;

        [Inject] protected ISoloState SoloState { get; set; } = default!;
        [Inject] protected IApiService TeamApi { get; set; } = default!;

        private string culture = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
        private bool _isLoaded = false;

        /// <summary>
        /// Játék típusok gombjai 24-26-ig vannak indexelve  
        /// Kategoriák gombjai 0-15-ig vannak indexelve
        /// Orientáció gombjai 16-23-ig vannak indexelve
        /// </summary>
        private const int RootButtonCount = 3;
        private const int CatBtnCount = 16;
        private const int OriBtnCount = 8;
        private int _buttonAryStart = CatBtnCount+ OriBtnCount;
        private int _buttonAryEnd = CatBtnCount + OriBtnCount + RootButtonCount;
        private string SubHeaderTitle = string.Empty;
        private Dictionary<string,ContentBoxVm>? _soloBtns;

        //private ContentBoxVm soloBtn(int idx) => _soloBtns![idx];

        private void BuildButtons() 
        {
            if (SoloState.Snapshot != null)
            {
                _soloBtns = SgameBtnBuilder.BuildBoxes(SoloState.Snapshot!, culture, Lang);
                _isLoaded = true;
            }
        }

        private void OnBtnClick(int btnId)
        {
            Console.WriteLine($"megnyomta a paraszt következő gombot: {btnId}");
            /*
                switch (btnId) 
            {
                case  1: _buttonAryStart = CatBtnCount + OriBtnCount; _buttonAryEnd = CatBtnCount + OriBtnCount + RootButtonCount; 
                        SubHeaderTitle=string.Empty; 
                    break;
                case 10: _buttonAryStart =  0; _buttonAryEnd = CatBtnCount;
                    SubHeaderTitle = _soloBtns[CatBtnCount + OriBtnCount].Header ?? "";
                    break;
                case 11: _buttonAryStart = CatBtnCount; _buttonAryEnd = CatBtnCount+OriBtnCount;
                    SubHeaderTitle = _soloBtns[CatBtnCount + OriBtnCount+1].Header ?? "";
                    break;
            }
             */

            Console.WriteLine($"megnyomta a paraszt következő gombot: {SubHeaderTitle}");
        }
        private void OnHeadButtonClick()
        {
            OnBtnClick(1);
        }
        protected override async Task OnInitializedAsync()
        {
            
            await SoloState.EnsureLoadedAsync();
            PageTitle.SetTitle(Lang["mainlayout.Header.GameSolo"],3, -1);
            //_isLoaded = true;
            if (_isLoaded==false)
                {
                    BuildButtons();
                }
            
        }
    }
}

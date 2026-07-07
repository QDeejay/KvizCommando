using KvizCommando.Client.Features.Team;
using KvizCommando.Client.Models.ViewModels;
using KvizCommando.Client.Services.Visual.UiService.Language;
using KvizCommando.Shared.Models.Dtos;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


namespace KvizCommando.Client.Components.Dynamic
{
    public partial class TModalRender
    {
        // [Inject] private ITeamModalDataBuilder render { get; set; } = default!;
        [Inject] private ILanguageService Lang { get; set; } = default!;
        [Parameter] public int mode { get; set; } = default!;

        [Parameter] public CandidateDto Candidate { get; set; } = default!;

        [Parameter] public TeamMemberDto TeamMember { get; set; } = default!;

        [Parameter] public int canNo { get; set; } = default!;
        [Parameter] public int tabPosH { get; set; } = default!;
        private string culture = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
        private bool isLoaded = false;
        private InfoBlock Info = default!;
        private RankHeader rh = default!;

        private TBuilderModal? _builder;
        ModalHireVm _vmHir = new();
        ModalPromoteVm _vmPro = new();
        ModalRetireVm _vmRet = new();
        ModalHandleVm _vmHan = new();
        protected override void OnParametersSet()
        {
            switch (mode) 
            {
                case 1:
                    if (canNo > 0 && tabPosH>0)
                    {
                        _vmHir = _builder!.BuildHireVm(Candidate, tabPosH, canNo, culture);
                        Info = _vmHir.Info;
                        rh = new RankHeader();
                    }
                    break;
                case 2:
                    _vmPro = _builder!.BuildPromoteVm(TeamMember, culture);
                    Info = _vmPro.Info;
                    rh = new RankHeader()
                    {
                        Rank = _vmPro.UnlocksRank,
                        RankClass = _vmPro.RankClass,
                        Level = _vmPro.UnlocksLevel,
                        NewClass = _vmPro.RankClassChanged
                    };
                    break;
                case 3:
                    _vmRet = _builder!.BuildRetireVm(TeamMember, culture);
                    Info = _vmRet.Info;
                    rh = new RankHeader()
                    {
                        Rank = _vmRet.UnlocksRank,
                        RankClass = _vmRet.RankClass,
                        Level = _vmRet.UnlocksLevel,
                        NewClass = _vmRet.RankClassChanged
                    };
                    break;
                case 4:
                    _vmHan = _builder!.BuildHandleVm(TeamMember,culture);
                    Info= _vmHan.Info;
                    rh = new RankHeader();
                    break;
            }
        }
        protected override void OnInitialized()
        {
            _builder = new TBuilderModal(Lang);
            isLoaded = true;
        }
        private class RankHeader
        {
            public string RankClass { get; set; } = string.Empty;
            public string Rank { get; set; } = string.Empty;
            public string Level { get; set; } = string.Empty;
            public bool NewClass { get; set; } = false;
        }
    }
}

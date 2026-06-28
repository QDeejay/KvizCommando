using KvizCommando.Client.Features.Team;
using KvizCommando.Client.Models.ViewModels;
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


namespace KvizCommando.Client.Pages.Team.Components
{
    public partial class TeamModalRender
    {
        [Inject] private ITeamModalDataBuilder render { get; set; } = default!;
        [Parameter] public int mode { get; set; } = default!;

        [Parameter] public CandidateDto Candidate { get; set; } = default!;

        [Parameter] public TeamMemberDto TeamMember { get; set; } = default!;

        [Parameter] public int canNo { get; set; } = default!;
        [Parameter] public int tabPosH { get; set; } = default!;
        private string culture = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
        private bool isLoaded = false;
        private InfoBlock Info = default!;
        private RankHeader rh = default!;
        HireModalViewModel vmHir = new();
        PromoteModalView vmPro = new();
        RetireModalView vmRet = new();
        HandleModalView vmHan = new();
        protected override void OnParametersSet()
        {
            switch (mode) 
            {
                case 1:
                    if (canNo > 0 && tabPosH>0)
                    {
                        vmHir = render.BuildHire(Candidate, tabPosH, canNo, culture);
                        Info = vmHir.Info;
                        rh = new RankHeader();
                    }
                    break;
                case 2:
                    vmPro = render.BuildPromote(TeamMember, culture);
                    Info = vmPro.Info;
                    rh = new RankHeader()
                    {
                        Rank = vmPro.UnlocksRank,
                        RankClass = vmPro.RankClass,
                        Level = vmPro.UnlocksLevel,
                        NewClass = vmPro.RankClassChanged
                    };
                    break;
                case 3:
                    vmRet = render.BuildRetire(TeamMember, culture);
                    Info = vmRet.Info;
                    rh = new RankHeader()
                    {
                        Rank = vmRet.UnlocksRank,
                        RankClass = vmRet.RankClass,
                        Level = vmRet.UnlocksLevel,
                        NewClass = vmRet.RankClassChanged
                    };
                    break;
                case 4:
                    vmHan = render.BuildHandle(TeamMember,culture);
                    Info= vmHan.Info;
                    rh = new RankHeader();
                    break;
            }
        }
        protected override void OnInitialized()
        {
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

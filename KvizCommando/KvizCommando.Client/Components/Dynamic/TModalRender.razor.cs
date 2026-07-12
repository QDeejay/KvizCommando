using KvizCommando.Client.Features.Modal;
using KvizCommando.Client.Features.Team;
using KvizCommando.Client.Models.ViewModels;
using KvizCommando.Client.Services.ClientCache;
using KvizCommando.Client.Services.Visual.UiService.Language;
using KvizCommando.Shared.Models.Dtos;
using Microsoft.AspNetCore.Components;


namespace KvizCommando.Client.Components.Dynamic
{
    public partial class TModalRender
    {
        [Inject] private ILanguageService Lang { get; set; } = default!;

        [CascadingParameter]
        public AppState AppStates { get; set; } = default!;

        [CascadingParameter]
        public ModalTypes Mode { get; set; } = ModalTypes.None;

        [Parameter] public int CanDidateNo { get; set; } = default!;
        [Parameter] public int SelectedMember { get; set; } = default!;
        private bool _isLoaded = false;
        private string Culture => AppStates.Culture;
        private TeamMemberDto Member => SelectedMember > 0 && SelectedMember <= 8 ? AppStates.Team.TeamMembers[SelectedMember] : new();
        private CandidateDto Candidate => CanDidateNo > 0 && CanDidateNo <= 8 ? AppStates.Team.Candidates[CanDidateNo] : new();

        private InfoBlock Info = default!;
        private RankHeader rh = default!;

        private TBuilderModal? _builder;
        ModalHireVm _vmHir = new();
        ModalPromoteVm _vmPro = new();
        ModalRetireVm _vmRet = new();
        ModalHandleVm _vmHan = new();
        protected override void OnParametersSet()
        {
            switch (Mode)
            {
                case ModalTypes.THire:
                    if (CanDidateNo > 0 && SelectedMember > 0)
                    {
                        _vmHir = _builder!.BuildHireVm(Candidate, SelectedMember, CanDidateNo, Culture);
                        Info = _vmHir.Info;
                        rh = new RankHeader();
                    }
                    break;
                case ModalTypes.TPromote:
                    _vmPro = _builder!.BuildPromoteVm(Member, Culture);
                    Info = _vmPro.Info;
                    rh = new RankHeader()
                    {
                        Rank = _vmPro.UnlocksRank,
                        RankClass = _vmPro.RankClass,
                        Level = _vmPro.UnlocksLevel,
                        NewClass = _vmPro.RankClassChanged
                    };
                    break;
                case ModalTypes.TRetire:
                    _vmRet = _builder!.BuildRetireVm(Member, Culture);
                    Info = _vmRet.Info;
                    rh = new RankHeader()
                    {
                        Rank = _vmRet.UnlocksRank,
                        RankClass = _vmRet.RankClass,
                        Level = _vmRet.UnlocksLevel,
                        NewClass = _vmRet.RankClassChanged
                    };
                    break;
                case ModalTypes.THandle:
                    _vmHan = _builder!.BuildHandleVm(Member, Culture);
                    Info = _vmHan.Info;
                    rh = new RankHeader();
                    break;
            }
        }
        protected override void OnInitialized()
        {
            _builder = new TBuilderModal(Lang);
            _isLoaded = true;
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

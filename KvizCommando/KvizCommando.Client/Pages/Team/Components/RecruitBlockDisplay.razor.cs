using KvizCommando.Client.Features.Team;
using KvizCommando.Client.Models.ViewModels;
using KvizCommando.Client.Services.ClientCache;
using KvizCommando.Client.Utilities;
using KvizCommando.Shared.Models.Dtos;
using Microsoft.AspNetCore.Components;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace KvizCommando.Client.Pages.Team.Components
{
    public partial class RecruitBlockDisplay : KcComponentBase, IDisposable
    {
        [CascadingParameter]
        private AppState AppStates { get; set; } = default!;

        [CascadingParameter]
        public int SelectedPos { get; set; } = default!;

        [Parameter] public EventCallback<int> OnCandidateHired { get; set; } = default!;
        [Parameter] public int[] CandidateOrder { get; set; } = default!;

        private RecruitVm vm = new();
        private int SelectedId = 0;
        private int _prevTabPosH = -1;
        private bool _couldBeHire = false;
        private string Culture => AppStates.Culture;

        private CandidateDto RecruitData => SelectedPos > 0 ? AppStates.Team.Candidates[SelectedPos] : new();
        protected override void OnParametersSet()
        {
            _couldBeHire = false;
            if (SelectedPos >= 1 && SelectedPos <= 8 && RecruitData.CanBeHire)
            {
                _couldBeHire = RecruitData.CanBeHire;
                if (SelectedPos != _prevTabPosH)
                {
                    SelectedId = 0;
                    _prevTabPosH = SelectedPos;
                    vm = TBuilderRecruit.BuildRecruitVm(RecruitData, CandidateOrder, SelectedPos, Culture, Ui.Lang);
                }

            }

        }
        private async Task OnHireButtnnAsync()
        {
            int delegateitem = SelectedPos*100+ SelectedId;
 
            if (OnCandidateHired.HasDelegate)
                await OnCandidateHired.InvokeAsync(delegateitem);
        }
        private void OnCandidateSelect(int id)
        {
            if (SelectedId == id) return;
            SelectedId = id;
        }


        public void Dispose()
        {
            OnCandidateHired = default;
            GC.SuppressFinalize(this);
        }
    }
}


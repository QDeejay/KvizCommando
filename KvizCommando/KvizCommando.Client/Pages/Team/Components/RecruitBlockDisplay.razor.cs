using KvizCommando.Client.Models.ViewModels;
using KvizCommando.Client.Utilities;
using KvizCommando.Shared.Models.Dtos;
using Microsoft.AspNetCore.Components;
using System.Globalization;

namespace KvizCommando.Client.Pages.Team.Components
{
    public partial class RecruitBlockDisplay : KcComponentBase, IDisposable
    {
        //[Inject] private RecruitBlockBuilder Builder { get; set; } = default!;

        [Parameter] public CandidateDto RecruitData { get; set; } = default!;
        [Parameter] public int TabPos { get; set; } = default!;
        [Parameter] public EventCallback<int> SelectedCandidate { get; set; } = default!;
        [Parameter] public int[] candidateOrder { get; set; } = default!;
        private RecruitVm vm = new();
        private int SelectedId = 0;
        private int _prevTabPosH = -1;
        private bool _couldBeHire = false;
        private string culture = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
        protected override void OnParametersSet()
        {
            _couldBeHire = false;
            if (TabPos >= 1 && TabPos <= 8 && RecruitData.CanBeHire)
            {
                _couldBeHire = RecruitData.CanBeHire;
                if (TabPos != _prevTabPosH)
                {
                    SelectedId = 0;
                    _prevTabPosH = TabPos;
                    // vm = RecruitBlockBuilder.Build(RecruitData, candidateOrder, TabPos, culture, Ui.Lang);
                }

            }

        }
        protected async Task OnCandidateSelected(int candidateId)
        {
            if (SelectedId == candidateId) return;
            SelectedId = candidateId;
            if (SelectedCandidate.HasDelegate)
                await SelectedCandidate.InvokeAsync(candidateId);
        }

        public void Dispose()
        {
            SelectedCandidate = default;
            GC.SuppressFinalize(this);
        }
    }
}


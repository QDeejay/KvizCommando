using KvizCommando.Client.Models.ViewModels;
using KvizCommando.Client.Pages.Team.Dynamic.Builders;
using KvizCommando.Client.Services.ClientCache;
using KvizCommando.Client.Services.Visual.UiService.Language;
using KvizCommando.Shared.Models.Dtos;
using Microsoft.AspNetCore.Components;

namespace KvizCommando.Client.Pages.Team.Dynamic
{
    public partial class RecruitManager : IDisposable
    {
        [Inject] ILanguageService Lang { get; set; } = default!;

        [CascadingParameter]
        private AppState AppStates { get; set; } = default!;

        [CascadingParameter]
        public int SelectedPos { get; set; } = default!;

        [Parameter] public Func<int, Task>? OnCandidateHired { get; set; }
        //[Parameter] public EventCallback<int> OnCandidateHired { get; set; } = default!;
        [Parameter] public int[] CandidateOrder { get; set; } = default!;

        private RecruitVm _vm = new();
        private int _selectedId = 0;
        private int _prevTabPosH = -1;
        private bool _couldBeHire = false;
        private string _picCode = string.Empty;
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
                    _selectedId = 0;
                    _picCode = string.Empty;
                    _prevTabPosH = SelectedPos;
                    _vm = TBuilderRecruit.BuildRecruitVm(RecruitData, CandidateOrder, SelectedPos, Culture, Lang);
                }

            }

        }
        private async Task OnHireButtonAsync()
        {
            int delegateitem = SelectedPos * 100 + _selectedId;

            if (OnCandidateHired is not null)
                await OnCandidateHired.Invoke(delegateitem);

            //if (OnCandidateHired.HasDelegate)
            //  await OnCandidateHired.InvokeAsync(delegateitem);
        }
        private void OnCandidateSelect(int id)
        {
            if (_selectedId == id) return;
            _selectedId = id;
            _picCode = RecruitData.PictureCode[id - 1];

        }

        public void Dispose()
        {
            OnCandidateHired = default;
            GC.SuppressFinalize(this);
        }
    }
}


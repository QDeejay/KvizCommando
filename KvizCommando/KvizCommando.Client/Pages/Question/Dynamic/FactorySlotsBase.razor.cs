using KvizCommando.Client.Helpers;
using KvizCommando.Client.Services.ClientCache;
using KvizCommando.Client.Services.Visual;
using KvizCommando.Client.Services.Visual.UiService.Language;
using KvizCommando.Shared.Models.Dtos;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace KvizCommando.Client.Pages.Question.Dynamic
{
    public partial class FactorySlotsBase : IDisposable
    {
        [Inject] ILanguageService Lang { get; set; } = default!;
        [CascadingParameter]
        private AppState AppStates { get; set; } = default!;
        [Inject] private CategoryOptionHelpers CatHelper { get; set; } = default!;
        [Parameter] public Func<int[], Task>? SaveSlots { get; set; }

        private const int ROW_COUNT = 10;

        private int? _editingRowIndex;
        private CategoryOption[] _options = [];
        private int[] _originalCodes = [];
        private int[] _workingCodes = [];

        private string Culture => AppStates.Culture;
        private bool IsDirty => !QuestionHelper.ArraysEqual(_originalCodes, _workingCodes);
        private int[] FactSlots => AppStates.Question!.FactorySlots;
        private QuestionExtendedInfo ExtInfo => AppStates.Question!.ExtendedInfo;

        protected override void OnInitialized()
        {
            (_originalCodes, _workingCodes) = QuestionHelper.CloneFactorySlots(FactSlots);
        }
        private void StartEdit(int rowIndex)
        {
            _editingRowIndex = rowIndex;
            StateHasChanged();
        }
        private void StopEdit()
        {
            _editingRowIndex = null;
            StateHasChanged();
        }
        private async Task OnSaveSlotsAsync()
        {
            if (!IsDirty) return;
            StopEdit();
            if (SaveSlots is not null)
                await SaveSlots.Invoke(_workingCodes);
            (_originalCodes, _workingCodes) = QuestionHelper.CloneFactorySlots(FactSlots);
        }
        private void OnEditorKeyDown(KeyboardEventArgs e)
        {
            if (e.Key == "Enter" || e.Key == "Escape")
                StopEdit();
        }
        public void Dispose()
        {
            SaveSlots = default;
            GC.SuppressFinalize(this);
        }
    }
}

//(_originalCodes, _workingCodes) = QuestionHelper.CloneFactorySlots(FactSlots);
//[Parameter] public EventCallback<int[]> SaveSlots { get; set; } = default!;
//if (SaveSlots.HasDelegate)
//  await SaveSlots.InvokeAsync(_workingCodes);
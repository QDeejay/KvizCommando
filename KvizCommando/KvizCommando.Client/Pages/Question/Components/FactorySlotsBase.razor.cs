using KvizCommando.Client.Helpers;
using KvizCommando.Client.Services.ClientCache;
using KvizCommando.Client.Services.Dto;
using KvizCommando.Client.Services.Visual;
using KvizCommando.Client.Utilities;
using KvizCommando.Shared.Contracts.Question;
using KvizCommando.Shared.Models.Dtos;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System.Globalization;
using System.Threading.Tasks;

namespace KvizCommando.Client.Pages.Question.Components
{
    public partial class FactorySlotsBase : KcComponentBase, IDisposable
    {

        [CascadingParameter]
        private AppState AppStates { get; set; } = default!;
       
        [Inject] private CategoryOptionHelpers CatHelper { get; set; } = default!;
        [Parameter] public EventCallback<int[]> SaveSlots { get; set; } = default!;

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
        protected void StartEdit(int rowIndex)
        {
            _editingRowIndex = rowIndex;
            StateHasChanged();
        }
        protected void StopEdit()
        {
            _editingRowIndex = null;
            StateHasChanged();
        }
        protected async Task OnSaveSlots()
        {
            if (!IsDirty) return;
            StopEdit();
            if (SaveSlots.HasDelegate)
                await SaveSlots.InvokeAsync(_workingCodes);
            (_originalCodes, _workingCodes) = QuestionHelper.CloneFactorySlots(FactSlots);
        }
        protected void OnEditorKeyDown(KeyboardEventArgs e)
        {
            if (e.Key == "Enter" || e.Key == "Escape")
                StopEdit();
        }
        public void Dispose()
        {
            SaveSlots =default;
            GC.SuppressFinalize(this);
        }
    }
}
/*
         //[Inject] protected ILanguageService Lang { get; set; } = default!;
        //[Parameter] public QuestionExtendedInfo ExtInfo { get; set; } = default!;
        //[Parameter] public int[] WorkingCodes { get; set; } = Array.Empty<int>();
        //[Parameter] public int[] OriginalCodes { get; set; } = Array.Empty<int>();
         //[Parameter] public string MessageFact { get; set; } = string.Empty;
        //[Parameter] public bool isSuccess { get; set; } = false;
 */
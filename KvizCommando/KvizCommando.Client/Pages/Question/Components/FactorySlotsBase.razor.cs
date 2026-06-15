using KvizCommando.Client.Helpers;
using KvizCommando.Client.Services.Dto;
using KvizCommando.Client.Services.Language;
using KvizCommando.Client.Services.Visual;
using KvizCommando.Shared.Contracts.Question;
using KvizCommando.Shared.Models.Dtos;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System.Globalization;
using System.Threading.Tasks;

namespace KvizCommando.Client.Pages.Question.Components
{
    public partial class FactorySlotsBase : ComponentBase, IDisposable
    {
        [Inject] protected ILanguageService Lang { get; set; } = default!;
        [Inject] protected CategoryOptionHelpers CatHelper { get; set; } = default!;

        [Parameter] public QuestionExtendedInfo ExtInfo { get; set; } = default!;
        [Parameter] public int[] WorkingCodes { get; set; } = Array.Empty<int>();
        [Parameter] public int[] OriginalCodes { get; set; } = Array.Empty<int>();
        [Parameter] public string MessageFact { get; set; } = string.Empty;
        [Parameter] public bool isSuccess { get; set; } = false;
        [Parameter] public EventCallback SaveSlots { get; set; }

        private bool IsDirty => !QuestionHelper.ArraysEqual(OriginalCodes, WorkingCodes);
        protected string culture = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
        protected CategoryOption[] Options = Array.Empty<CategoryOption>();
        protected int? EditingRowIndex;
        protected const int RowCount = 10;

        protected override void OnInitialized()
        {
        }
        protected void StartEdit(int rowIndex)
        {
            EditingRowIndex = rowIndex;
            StateHasChanged();
        }
        protected void StopEdit()
        {
            EditingRowIndex = null;
            StateHasChanged();
        }
        protected async Task OnSaveSlots()
        {
            if (!IsDirty) return;
            StopEdit();
            if (SaveSlots.HasDelegate)
                await SaveSlots.InvokeAsync(true);  
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
 
 
 */
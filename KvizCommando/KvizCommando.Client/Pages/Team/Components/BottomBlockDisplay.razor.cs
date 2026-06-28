using KvizCommando.Client.Features.Team;
using KvizCommando.Client.Models.ViewModels;
using KvizCommando.Shared.Models.Dtos;
using Microsoft.AspNetCore.Components;
using System.Globalization;
using System.Reflection.PortableExecutable;


namespace KvizCommando.Client.Pages.Team.Components
{
    public partial class BottomBlockDisplay
    {
        [Inject] private BottomBlockDataBuilder Builder { get; set; } = default!;
        [Parameter] public TeamDtos InputData { get; set; } = default!;
        [Parameter] public int TabPos { get; set; }
        [Parameter] public EventCallback<int> ActionButtonPushed { get; set; } = default!;
        private string colorColumn { get; set; } = string.Empty;
        private string culture = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
        private BottomBlockViewModel vm = new();
        protected override void OnParametersSet()
        {
            if (TabPos == 0 || InputData.CharCatMask[TabPos])
                vm = Builder.Build(InputData, TabPos, culture);
        }
        protected async Task OnActionButtonPushed(int rowId)
        {
            int delegateItem = vm.Rows[rowId].action > 400 ? vm.Rows[rowId].action - 100 : vm.Rows[rowId].action;
            if (ActionButtonPushed.HasDelegate)
                await ActionButtonPushed.InvokeAsync(delegateItem);
        }
        public void Dispose()
        {
            ActionButtonPushed = default;
            GC.SuppressFinalize(this);
        }
    }
    
}
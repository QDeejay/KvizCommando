using KvizCommando.Client.Models.ViewModels;
using Microsoft.AspNetCore.Components;


namespace KvizCommando.Client.Components
{
    public partial class RecruitCard
    {

        [Parameter] public RecruitCardVm Card { get; set; } = default!;
        [Parameter] public int CardNo { get; set; } = 0;
        [Parameter] public int SelectedCardNo { get; set; } = 0;
        [Parameter] public EventCallback<int> Selected { get; set; } = default!;

        private bool _isSelected = false;
        private RecruitCardVm Vm => Card;

        protected override void OnParametersSet()
        {
            _isSelected = CardNo == SelectedCardNo;
        }

        private async Task OnCardSelected()
        {

            if (Selected.HasDelegate)
                await Selected.InvokeAsync(CardNo);
        }

    }

}

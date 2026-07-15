using KvizCommando.Client.Models.ViewModels;
using Microsoft.AspNetCore.Components;
using System.Reflection.Metadata;


namespace KvizCommando.Client.Components
{
    public partial class RecruitCard : IDisposable
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
            // if (_vm.Name != Card.Name)
            // {
            //   _vm = Card;
            // }
        }

        private async Task OnCardSelected()
        {

            if (Selected.HasDelegate)
                await Selected.InvokeAsync(CardNo);
        }

        public void Dispose()
        {
            Selected = default;
            GC.SuppressFinalize(this);
        }

    }

}

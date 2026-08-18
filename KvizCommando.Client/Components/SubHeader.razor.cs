using KvizCommando.Client.Models.ViewModels.Ui;
using Microsoft.AspNetCore.Components;


namespace KvizCommando.Client.Components
{
    public partial class SubHeader
    {
        [Parameter] public IReadOnlyList<SubHeaderVm> Tabs { get; set; } = default!;
        [Parameter] public int StartIndex { get; set; } = 0;
        [Parameter] public bool IsVisible { get; set; } = false;

        [Parameter] public EventCallback<int> ActiveIndexChanged { get; set; }

        private bool _isActive => IsVisible;
        private int _activeIndex = 0;
        private int _previousStartIndex = 0;
        protected override void OnParametersSet()
        {
            if (_previousStartIndex != StartIndex)
            {
                _activeIndex = StartIndex;
                _previousStartIndex = StartIndex;
            }
        }
        private async Task SetTab(int index)
        {
            if (!Tabs[index - 1].Enable)
                return;
            if (!ActiveIndexChanged.HasDelegate)
                return;
            _activeIndex = index;
            await ActiveIndexChanged.InvokeAsync(index);
        }


    }
}

using KvizCommando.Client.Models.ViewModels.Ui;
using KvizCommando.Client.Services.Visual.UiService.Language;
using Microsoft.AspNetCore.Components;


namespace KvizCommando.Client.Components
{
    public partial class SubHeader
    {
        [Inject] ILanguageService Lang { get; set; } = default!;
        //[Parameter] public string[] Tabs { get; set; } = default!;
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
/*
  <div class="htp-tab @(SubHeaderPar.ActiveIndex == 1 ? "active" : "") @(SubHeaderPar.TabEnable[1] ? "" : "disabled")" @onclick="() => SetTab(1)" disabled="@(!SubHeaderPar.TabEnable[1])">
            @SubHeaderPar.HTabs[1]
            <span class="tooltip-box">@SubHeaderPar.TabDisableText[1]</span>
        </div>
        <div class="htp-tab @(SubHeaderPar.ActiveIndex == 2 ? "active" : "") @(SubHeaderPar.TabEnable[2] ? "" : "disabled")" @onclick="() => SetTab(2)" disabled="@(!SubHeaderPar.TabEnable[2])">
            @SubHeaderPar.HTabs[2]
            <span class="tooltip-box">@SubHeaderPar.TabDisableText[2]</span>
        </div>
        <div class="htp-tab @(SubHeaderPar.ActiveIndex == 3 ? "active" : "") @(SubHeaderPar.TabEnable[3] ? "" : "disabled")" @onclick="() => SetTab(3)" disabled="@(!SubHeaderPar.TabEnable[3])">
            @SubHeaderPar.HTabs[3]
            <span class="tooltip-box">@SubHeaderPar.TabDisableText[3]</span>
        </div>
        <div class="htp-tab @(SubHeaderPar.ActiveIndex == 4 ? "active" : "") @(SubHeaderPar.TabEnable[4] ? "" : "disabled")" @onclick="() => SetTab(4)" disabled="@(!SubHeaderPar.TabEnable[4])">
            @SubHeaderPar.HTabs[4]
            <span class="tooltip-box">@SubHeaderPar.TabDisableText[4] </span>
        </div>
        <div class="htp-tab @(SubHeaderPar.ActiveIndex == 5 ? "active" : "") @(SubHeaderPar.TabEnable[5] ? "" : "disabled")" @onclick="() => SetTab(5)" disabled="@(!SubHeaderPar.TabEnable[5])">
            @SubHeaderPar.HTabs[5]
            <span class="tooltip-box">@SubHeaderPar.TabDisableText[5] </span>
        </div>
        <div class="htp-tab @(SubHeaderPar.ActiveIndex == 6 ? "active" : "") @(SubHeaderPar.TabEnable[6] ? "" : "disabled")" @onclick="() => SetTab(6)" disabled="@(!SubHeaderPar.TabEnable[6])">
            @SubHeaderPar.HTabs[6]
            <span class="tooltip-box">@SubHeaderPar.TabDisableText[6] </span>
        </div>
        <div class="htp-tab @(SubHeaderPar.ActiveIndex == 7 ? "active" : "") @(SubHeaderPar.TabEnable[7] ? "" : "disabled")" @onclick="() => SetTab(7)" disabled="@(!SubHeaderPar.TabEnable[7])">
            @SubHeaderPar.HTabs[7]
            <span class="tooltip-box">@SubHeaderPar.TabDisableText[7] </span>
        </div>
        <div class="htp-tab @(SubHeaderPar.ActiveIndex == 8 ? "active" : "") @(SubHeaderPar.TabEnable[8] ? "" : "disabled")" @onclick="() => SetTab(8)" disabled="@(!SubHeaderPar.TabEnable[8])">
            @SubHeaderPar.HTabs[8]
            <span class="tooltip-box">@SubHeaderPar.TabDisableText[8] </span>
        </div>
    </div>
 
 */
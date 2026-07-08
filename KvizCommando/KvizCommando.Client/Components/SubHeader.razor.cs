using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Threading.Tasks;


namespace KvizCommando.Client.Components
{
    public partial class SubHeader
    {


        [Parameter] public EventCallback<int> ActiveIndexChanged { get; set; }
        [Parameter] public SubHeaderParams Params { get; set; } = default!;
        
        private bool _isActive = false;
 
        protected override void OnParametersSet()
        {
            _isActive = Params != null && Params.ActiveIndex > 0 && Params.ActiveIndex < 9;
        }
        private async Task SetTab(int index)
        {
            if (!Params.TabEnable[index])
                return;
            Params.ActiveIndex = index;
            await ActiveIndexChanged.InvokeAsync(index);
        }


    }

    public sealed class SubHeaderParams
    {
        public int[] EnabledTabs { get; set; } = new int[9];
        public string[] HTabs { get; set; } = new string[9];
        public int ActiveIndex { get; set; } = 0;
        public bool[] TabEnable { get; set; } = new bool[9];
        public string[] TabDisableText { get; set; } = new string[9];
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
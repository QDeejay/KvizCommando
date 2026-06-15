using KvizCommando.Client.Services.Dto;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Primitives;
using System;
using System.Reflection;

namespace KvizCommando.Client.Components
{
    public partial class HorizontalTabPane : ComponentBase
    {
        [Parameter] public int ActiveHIndex { get; set; }
        [Parameter] public int ActiveVIndex { get; set; }
        [Parameter] public (bool[]?, string[]?) TabHEnabled { get; set; }
        [Parameter] public string? TabTitle { get; set; }
        [Parameter] public bool PicTabEnabled { get; set; } 
        [Parameter] public EventCallback<int> ActiveHIndexChanged { get; set; }
        [Parameter] public EventCallback<int> ActiveVIndexChanged { get; set; }
        [Parameter] public string[]? HTabs { get; set; }
        [Parameter] public string[]? VTabs { get; set; }
        [Parameter] public RenderFragment? Content1 { get; set; }
        [Parameter] public RenderFragment? Content2 { get; set; }
        [Parameter] public RenderFragment? Content3 { get; set; }
        [Parameter] public RenderFragment? Content4 { get; set; }
        [Parameter] public RenderFragment? Content5 { get; set; }
        [Parameter] public RenderFragment? ButtonRow { get; set; }

        private bool picTabVisible => PicTabEnabled;
        private string title => TabTitle ?? "";
        
        private bool[] TabVEnabled => BuildTabVEnabled();

        private async Task SetHTab(int index)
        {
            if (!TabHEnabled.Item1[index])
                return;
            ActiveHIndex = index;
            await ActiveHIndexChanged.InvokeAsync(index);
        }
        private async Task SetVTab(int index)
        {
            if (!TabVEnabled[index])
                return;
            ActiveVIndex = index;
            await ActiveVIndexChanged.InvokeAsync(index);
        }
        private bool[] BuildTabVEnabled()
        {
            var result = new bool[4];
            result[0] = ActiveVIndex != 3;
            result[1] = ActiveVIndex != 3;
            result[2] = ActiveHIndex > 0 && ActiveVIndex != 3;
            result[3] = ActiveHIndex > 0 && ActiveVIndex == 3;
            return result;
        }

    }
}

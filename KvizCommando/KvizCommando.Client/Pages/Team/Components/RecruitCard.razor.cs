using KvizCommando.Client.Models.ViewModels;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading.Tasks;


namespace KvizCommando.Client.Pages.Team.Components
{
    public partial class RecruitCard
    {

        [Parameter] public string CardName { get; set; } = string.Empty;
        [Parameter] public string CardInfo1 { get; set; } = string.Empty;
        [Parameter] public string CardInfo2 { get; set; } = string.Empty;
        [Parameter] public string CardInfo3 { get; set; } = string.Empty;
        [Parameter] public string SecOrient { get; set; } = string.Empty;
        [Parameter] public bool Sex { get; set; } = false;
        [Parameter] public int  CardNo { get; set; } = 0;
        [Parameter] public int SelectedCardNo { get; set; } = 0;
        [Parameter] public EventCallback<int> Selected { get; set; } = default!;

        private bool isSelected = false;
        private RecruitCardEntity vm = new();
        protected override void OnParametersSet()
        {
            isSelected = CardNo == SelectedCardNo;
            if (vm.Name != CardName)
            {
                vm.Info1 = CardInfo1;
                vm.Info2 = CardInfo2;
                vm.Info3 = CardInfo3;
                vm.SecOrient = SecOrient;
                vm.Name = CardName;
                vm.Sex = Sex;
            }            
        }


        protected async Task OnCardSelected()
        {
            
            if (Selected.HasDelegate)
                await Selected.InvokeAsync(CardNo);
        }
        
    }
   
}

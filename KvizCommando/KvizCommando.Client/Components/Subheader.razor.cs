using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Threading.Tasks;


namespace KvizCommando.Client.Components
{
    public partial class Subheader : ComponentBase
    {
        [Parameter] public string Title { get; set; }
        [Parameter] public EventCallback HeadButton { get; set; }

        private string SubHeaderTtle = string.Empty;
        private bool ShowSubHeaderEna => !string.IsNullOrEmpty(Title);

        protected override void OnParametersSet()
        {
            Console.WriteLine($"Subheader paraméterei frissültek: {Title}");
            SubHeaderTtle = Title;
        }

        private void OnHeadButtonClick()
        {
            if (HeadButton.HasDelegate)
            {
                HeadButton.InvokeAsync();
            }
        }
    }
}

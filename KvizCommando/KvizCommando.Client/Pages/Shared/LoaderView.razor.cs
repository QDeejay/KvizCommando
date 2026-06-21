using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KvizCommando.Client.Services.Visual;


namespace KvizCommando.Client.Pages.Shared
{
    public partial class LoaderView : ComponentBase, IDisposable
    {
        [Inject]
        private LoaderService Loader { get; set; } = default!;

        private bool _visible;

        protected override void OnInitialized()
        {
            Loader.OnShow += Show;
            Loader.OnHide += Hide;

            _visible = Loader.IsVisible;
        }

        private void Show()
        {
            _visible = true;

            InvokeAsync(StateHasChanged);
            Console.WriteLine("Loader ON");
        }

        private void Hide()
        {
            _visible = false;

            InvokeAsync(StateHasChanged);
        }

        public void Dispose()
        {
            Loader.OnShow -= Show;
            Loader.OnHide -= Hide;
        }
    }
}

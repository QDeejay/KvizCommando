using KvizCommando.Client.Models.ViewModels;
using KvizCommando.Client.Services.Visual.UiService;
using KvizCommando.Client.Utilities;
using KvizCommando.Shared.Models.Enums;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace KvizCommando.Client.Pages.Shared
{
    public partial class KcToast : IDisposable
    {
        [Inject]
        private ToastService Toast { get; set; } = default!;

        //------------------------------------------------------

        protected override void OnInitialized()
        {
           Toast.OnChanged += ToastChanged;
        }

        //------------------------------------------------------

        private void ToastChanged()
        {
            InvokeAsync(StateHasChanged);
        }

        //------------------------------------------------------

        private void Close()
        {
            Toast.Close();
        }

        //------------------------------------------------------

        protected string GetCssClass()
        {
            return Toast.Current?.Type switch
            {
                ToastType.Error => "kc-toast-error",
                ToastType.Success => "kc-toast-success",
                ToastType.Warning => "kc-toast-warning",
                ToastType.Info => "kc-toast-info",
                _ => string.Empty
            };
        }

        //------------------------------------------------------

        public void Dispose()
        {
            Toast.OnChanged -= ToastChanged;
        }
    }
}

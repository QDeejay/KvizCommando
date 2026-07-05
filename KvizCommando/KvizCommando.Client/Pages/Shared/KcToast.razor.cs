using KvizCommando.Client.Services.Visual.UiService;
using KvizCommando.Shared.Models.Enums;
using Microsoft.AspNetCore.Components;


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
        protected string GetBiIcon()
        {
            return Toast.Current?.Type switch
            {
                ToastType.Error => "bi bi-x-circle-fill",
                ToastType.Success => "bi bi-check-circle-fill",
                ToastType.Warning => "bi bi-exclamation-triangle-fill",
                ToastType.Info => "bi bi-info-circle-fill",
                _ => string.Empty
            };

        }


        //------------------------------------------------------

        public void Dispose()
        {
            Toast.OnChanged -= ToastChanged;
            GC.SuppressFinalize(this);
        }
    }
}
/*
 @if (Toast.IsVisible && Toast.Current is not null)
{GC.SuppressFinalize(this);
    <div class="kc-toast @GetCssClass()">
       
    </div>
}

 
 */
using Blazored.LocalStorage;
using KvizCommando.Client.Features.Modal;
using KvizCommando.Client.Models.ViewModels;
using KvizCommando.Client.Services.ClientCache;
using KvizCommando.Client.Services.Visual.UiService;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Threading.Tasks;

namespace KvizCommando.Client.Pages.Shared
{
    public partial class KcModal : IDisposable
    {
        [Inject] private IJSRuntime JS { get; set; } = default!;
        [Inject] private ILocalStorageService LocalStorage { get; set; } = default!;
        [Parameter] public AppState Appstates { get; set; } = default!;
        [Parameter] public string Id { get; set; } = "kcModal";
        [Parameter] public RenderFragment? ChildContent { get; set; }
        [Parameter] public EventCallback OnAction1 { get; set; }
        [Parameter] public EventCallback OnAction2 { get; set; }
        [Parameter] public EventCallback OnCloseAction { get; set; }
        [Parameter] public EventCallback OnCheckBoxAction { get; set; }
        [Parameter] public EventCallback<ModalResult> OnModalAction { get; set; }

        private ModalBoxVm Par = new();
        private bool CanAccept { get; set; } = false;
        private bool _bottomReached;
        private bool CheckBox { get; set; }  = false;
        private sealed record ScrollMetrics(
            double ScrollTop, double ScrollHeight, double ClientHeight,
            double OffsetHeight, double BoxHeight);
        public async Task ShowAsync(ModalBoxVm par)
        {
            Par = par;
            CheckBox = false;
            CanAccept = !Par.CheckBottom;
            await InvokeAsync(StateHasChanged);
            await JS.InvokeVoidAsync("kcModal.show", $"#{Id}");
            if (Par.CheckBottom == true && _bottomReached!=true)
            {
                
                await Task.Delay(500);
                await CheckBottomAsync();

            }
           //else CanAccept = true;
        }
        public async Task HideAsync() 
        {
            Par = new ModalBoxVm() with { Mode = ModalTypes.None };
            if (OnCloseAction.HasDelegate)
                await OnCloseAction.InvokeAsync();
            if (OnModalAction.HasDelegate)
                await OnModalAction.InvokeAsync(ModalResult.Close);
            await JS.InvokeVoidAsync("kcModal.hide", $"#{Id}");
            CheckBox = false;
            CanAccept = false;
            _bottomReached = false;
        }  
        private async Task OnActionClicked1()
        {
            if (CheckBox == true)
            {
                await LocalStorage.SetItemAsync(Par.CheckBoxKey ?? "ModalChkAction", true);
                if (OnCheckBoxAction.HasDelegate)
                    await OnCheckBoxAction.InvokeAsync();
            }
            
            if (OnAction1.HasDelegate) 
                await OnAction1.InvokeAsync();
       
            if (OnModalAction.HasDelegate)
                await OnModalAction.InvokeAsync(ModalResult.Button1);

            
            await HideAsync();
        }
        private async Task OnActionClicked2()
        {
            if (OnAction2.HasDelegate)
                await OnAction2.InvokeAsync();
            if (OnModalAction.HasDelegate)
                await OnModalAction.InvokeAsync(ModalResult.Button2);
            await HideAsync();
        }
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            /*
            if (firstRender)
            {
                _bottomReached = false;
                 CanAccept = Par.CheckBottom!=true;
            }
             */
            await Task.Delay(5);
           
        }
        private async Task OnTermsScroll()
        {
            if (_bottomReached) return;       // ne pörögjünk feleslegesen
            await CheckBottomAsync();
        }
        private async Task CheckBottomAsync()
        {
            var m = await JS.InvokeAsync<ScrollMetrics?>("kcMeasure", "#termsBody");
            if (m is null) return;

            var atBottom = Math.Ceiling(m.ScrollTop + m.ClientHeight) >= Math.Floor(m.ScrollHeight) - 1;
            if (atBottom)
            {
                _bottomReached = true;
                CanAccept = true;
                StateHasChanged();
            }
        }
        public void Dispose()
        {
            OnAction1 = default;
            OnAction2 = default;
            OnCloseAction = default;
            OnCheckBoxAction = default;
            GC.SuppressFinalize(this);
        }
    }
}
/*
 
                            data-bs-dismiss="modal"
 
 */
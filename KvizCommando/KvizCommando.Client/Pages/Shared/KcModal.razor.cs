using Blazored.LocalStorage;
using KvizCommando.Client.Features.Question;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Threading.Tasks;

namespace KvizCommando.Client.Pages.Shared
{
    public partial class KcModal : IDisposable
    {
        [Inject] private IJSRuntime JS { get; set; } = default!;
        [Inject] private ILocalStorageService LocalStorage { get; set; } = default!;
        [Parameter] public string Id { get; set; } = "kcModal";
        [Parameter] public ModalPar Par { get; set; } = default!;
        
        //[Parameter] public string Title { get; set; } = "Modal title";
        //[Parameter] public string ActionText1 { get; set; } = string.Empty;
        //[Parameter] public string ActionText2 { get; set; } = string.Empty;
        //[Parameter] public string ActionButtonStyle1 { get; set; } = string.Empty;
        //[Parameter] public string ActionButtonStyle2 { get; set; } = string.Empty;
        //[Parameter] public string CloseText { get; set; } = string.Empty;
        //[Parameter] public string SizeClass { get; set; } = string.Empty; // modal-sm, modal-lg, modal-xl
        //[Parameter] public string CheckBoxText { get; set; } = string.Empty;
        //[Parameter] public string CheckBoxKey { get; set; } = string.Empty;
        //[Parameter] public bool CheckBottom { get; set; } = false;
        [Parameter] public RenderFragment? ChildContent { get; set; }
        [Parameter] public EventCallback OnAction1 { get; set; }
        [Parameter] public EventCallback OnAction2 { get; set; }
        [Parameter] public EventCallback OnCloseAction { get; set; }
        [Parameter] public EventCallback CheckBoxAction { get; set; }

        private bool CanAccept { get; set; } = false;
        private bool _bottomReached;
        private bool CheckBox { get; set; }  = false;
        private sealed record ScrollMetrics(
            double ScrollTop, double ScrollHeight, double ClientHeight,
            double OffsetHeight, double BoxHeight);
        public async Task ShowAsync()
        {
            CheckBox = false;
            CanAccept = false;
            await JS.InvokeVoidAsync("kcModal.show", $"#{Id}");
            if (Par.CheckBottom == true && _bottomReached!=true)
            {
                
                await Task.Delay(500);
                await CheckBottomAsync();

            }
            else CanAccept = true;
        }
        public async Task HideAsync() 
        {
            if (OnCloseAction.HasDelegate)
                await OnCloseAction.InvokeAsync();
            await JS.InvokeVoidAsync("kcModal.hide", $"#{Id}");
            CheckBox = false;
            CanAccept = false;
        }  
        private async Task OnActionClicked1()
        {
            if (OnAction1.HasDelegate)
            {
                if (CheckBox == true)
                {
                    await LocalStorage.SetItemAsync(Par.CheckBoxKey ?? "ModalChkAction", true);
                    if (CheckBoxAction.HasDelegate)
                        await CheckBoxAction.InvokeAsync();
                }
                await OnAction1.InvokeAsync();
            }
            
            await HideAsync();
        }
        private async Task OnActionClicked2()
        {
            if (OnAction2.HasDelegate)
                await OnAction2.InvokeAsync();

            await HideAsync();
        }
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                _bottomReached = false;
                 CanAccept = Par.CheckBottom!=true;
            }
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
            OnAction1 = default!;
            OnAction2 = default!;
            OnCloseAction = default!;
            CheckBoxAction = default!;
            GC.SuppressFinalize(this);
        }
    }
}

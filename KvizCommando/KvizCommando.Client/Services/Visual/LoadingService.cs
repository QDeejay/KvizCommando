using KvizCommando.Client.Services.Dto;
using Microsoft.JSInterop;
using static System.Net.WebRequestMethods;

namespace KvizCommando.Client.Services.Visual
{
    public class LoadingService : ILoadingService
    {

        private readonly IJSRuntime _js;


        public LoadingService(

             IJSRuntime js)
        {
            _js = js;
        }

        public async Task Show()
        {
            await _js.InvokeVoidAsync("kcLoader.show");
        }
        public async Task Hide()
        {
            await _js.InvokeVoidAsync("kcLoader.hide");

        }
        
        public async Task IsActive()
        {
            await _js.InvokeAsync<bool>("kcLoader.isActive");
        }

    }

}
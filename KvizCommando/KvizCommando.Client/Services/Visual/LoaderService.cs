using KvizCommando.Client.Services.Dto;
using Microsoft.JSInterop;
using System.ComponentModel.Design;
using static System.Net.WebRequestMethods;

namespace KvizCommando.Client.Services.Visual
{

    public sealed class LoaderService
    {


        public event Action? OnShow;
        public event Action? OnHide;

        public bool IsVisible { get; private set; }

        private bool _cancelShow;

        public async Task ShowAsync()
        {
            if (IsVisible)
                return;

            _cancelShow = false;

            await Task.Delay(500);

            if (_cancelShow)
                return;

            IsVisible = true;

            OnShow?.Invoke();
        }

        public async Task HideAsync()
        {
            _cancelShow = true;

            if (!IsVisible)
                return;

            await Task.Delay(1000);

            IsVisible = false;

            OnHide?.Invoke();
        }

    }

}
/*
 
 */

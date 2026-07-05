using KvizCommando.Client.Models.ViewModels;
using KvizCommando.Shared.Models.Enums;
using System.Threading.Tasks;

namespace KvizCommando.Client.Services.Visual.UiService
{
    public sealed class ToastService
    {
        private readonly Queue<ToastMessage> _queue = new();

        private int _version;

        public ToastMessage? Current { get; private set; }

        public bool IsVisible { get; private set; }

        public event Action? OnChanged;

        //----------------------------------------------------

        public void Success(string text)
            => _ = Show(text, ToastType.Success);

        public void Error(string text)
            => _ = Show(text, ToastType.Error);

        public void Brief(string text)
            => _ = Show(text, ToastType.Warning);

        public void Complete(string text)
            => _ = Show(text, ToastType.Info);

        //----------------------------------------------------

        public async Task Show(string text, ToastType type)
        {
            var toast = new ToastMessage
            {
                Text = text,
                Type = type
            };
            await Task.Delay(1000);
            if (!IsVisible)
            {
                ShowInternal(toast);
                return;
            }

            _queue.Enqueue(toast);
        }

        //----------------------------------------------------

        public void Close()
        {
            if (!IsVisible)
                return;

            _version++;

            IsVisible = false;
            OnChanged?.Invoke();

            _ = FinishCloseAsync();
        }


        //----------------------------------------------------

        private void ShowNext()
        {
            if (_queue.Count == 0)
                return;

            ShowInternal(_queue.Dequeue());
        }

        //----------------------------------------------------

        private void ShowInternal(ToastMessage toast)
        {
            Current = toast;
            IsVisible = true;
            Console.WriteLine($"Subscribers: {OnChanged?.GetInvocationList().Length ?? 0}");
            OnChanged?.Invoke();
           
            _ = AutoCloseAsync();
        }

        //----------------------------------------------------

        private async Task AutoCloseAsync()
        {
           
            int version = ++_version;

            await Task.Delay(3000);

            if (version != _version)
                return;

            Close();
        }
        private async Task FinishCloseAsync()
        {
            await Task.Delay(250);   // ugyanannyi mint a CSS hide animation

            Current = null;

            OnChanged?.Invoke();

            ShowNext();
        }
    }
}

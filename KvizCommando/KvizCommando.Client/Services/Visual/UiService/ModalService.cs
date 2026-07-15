using KvizCommando.Client.Pages.Shared.Modal.ViewModels;

namespace KvizCommando.Client.Services.Visual.UiService
{
    public sealed class ModalService
    {
        private TaskCompletionSource<ModalResult>? _tcs;

        public ModalBoxVm? Parameter { get; private set; }

        public event Action? OnModalShow;
        public event Action? OnModalHide;

        public Task<ModalResult> ShowAsync(ModalBoxVm param)
        {
            Parameter = param;

            _tcs = new TaskCompletionSource<ModalResult>();

            OnModalShow?.Invoke();

            return _tcs.Task;
        }

        public void SendResult(ModalResult result)
        {
            if (_tcs == null)
                return;

            _tcs.SetResult(result);

            Parameter = null;
            _tcs = null;

            OnModalHide?.Invoke();
        }

        public void Cancel()
        {
            SendResult(ModalResult.Close);
        }
    }

    public enum ModalResult
    {
        None,
        Button1,
        Button2,
        Close
    }
}

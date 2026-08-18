using KvizCommando.Client.Features.Shared.Modal.ViewModels;

namespace KvizCommando.Client.Services.Visual.UiService
{
    public sealed class ModalService
    {
        private TaskCompletionSource<ModalResult>? _tcs;

        public ModalBoxVm? Parameter { get; private set; }

        public event Action? OnModalShow;
        public event Action? OnModalHide;

        /// <summary>
        /// Megjeleníti a modális ablakot vagy a betöltésjelzőt.
        /// </summary>
        /// <param name="param">A modális ablak tartalmát és működését leíró paraméterek.</param>
        public Task<ModalResult> ShowAsync(ModalBoxVm param)
        {
            Parameter = param;

            _tcs = new TaskCompletionSource<ModalResult>();

            OnModalShow?.Invoke();

            return _tcs.Task;
        }

        /// <summary>
        /// Átadja a modális művelet eredményét a várakozó hívónak.
        /// </summary>
        /// <param name="result">A modális ablak hívójának visszaadott eredmény.</param>
        public void SendResult(ModalResult result)
        {
            var completion = _tcs;
            if (completion is null)
                return;

            Parameter = null;
            _tcs = null;

            OnModalHide?.Invoke();

            completion.SetResult(result);
        }

        /// <summary>
        /// Eredmény nélkül lezárja az aktuális modális műveletet.
        /// </summary>
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

using KvizCommando.Client.Features.Question;

namespace KvizCommando.Client.Services.Visual.UiService
{
    public class ModalService
    {
        public ModalPar? Parameter { get; private set; } = default!;

        public event Action? OnModalShow;
        public event Action? OnModalHide;
        public event Action<ModalResult, ModalPar>? OnResult;
        public void Show (ModalPar param) 
        { 
            Parameter = param;
            OnModalShow?.Invoke();
        }
        public void Hide() 
        {
            Parameter = null; 
            OnModalHide?.Invoke();
        }
        public void SendResult(ModalResult result) 
        { 
            if (Parameter==null)
                return;
            OnResult?.Invoke(result,Parameter);
            Hide();
        }
    }
    public enum ModalResult 
    { 
        None,
        Button1,
        Button1ViaChbox,
        Button2,
        Close
    }
}

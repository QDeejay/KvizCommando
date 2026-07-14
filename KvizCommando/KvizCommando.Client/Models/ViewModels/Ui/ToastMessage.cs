using KvizCommando.Shared.Models.Enums;

namespace KvizCommando.Client.Models.ViewModels.Ui
{
    public sealed record ToastMessage
    {
        public Guid Id { get; init; } = Guid.NewGuid();

        public ToastType Type { get; init; }

        public string Text { get; init; } = string.Empty;
    }
   
}

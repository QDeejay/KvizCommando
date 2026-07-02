
using KvizCommando.Shared.Models.Enums;

namespace KvizCommando.Client.Models.ViewModels
{
    public sealed record ToastMessage
    {
        public Guid Id { get; init; } = Guid.NewGuid();

        public ToastType Type { get; init; }

        public string Text { get; init; } = string.Empty;
    }
   
}

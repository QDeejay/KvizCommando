using KvizCommando.Shared.Models.Dtos;

namespace KvizCommando.Client.Services.ClientCache
{
    public interface IQuestionState
    {
        bool IsLoaded { get; }
        QuestionDtos? Snapshot { get; }

        int[]? FactorySlots { get; }
        UserSlot[]? Userlots { get; }
        PendingSlot[]? PendingSlots { get; }
        QuestionExtendedInfo? ExtendedInfo { get; }
      

        Task EnsureLoadedAsync();
        Task RefreshAsync();
        void Invalidate();
        void Clear();
    }
}

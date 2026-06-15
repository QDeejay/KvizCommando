using KvizCommando.Shared.Models.Dtos;

namespace KvizCommando.Client.Services.ClientCache
{
    public interface ISoloState
    {
        bool IsLoaded { get; }
        SoloGameDtos? Snapshot { get; }

        ResultDtos[]? CatResult { get; }
        ResultDtos[]? OriResult { get; }


        //bool[]? Orimask { get; }
        //bool[]? Catmask { get; }

        //int? CatOvr { get; }
        //int? OriOvr { get; }

        Task EnsureLoadedAsync();
        Task RefreshAsync();
        void Invalidate();
        void Clear();
    }
}

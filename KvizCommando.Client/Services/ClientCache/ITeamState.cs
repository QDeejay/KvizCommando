using KvizCommando.Shared.Models.Dtos;

namespace KvizCommando.Client.Services.ClientCache
{
    public interface ITeamState
    {
        bool IsLoaded { get; }
        TeamDtos? Snapshot { get; }
        TeamMemberDto[]? TeamMembers { get; }
        CandidateDto[]? Candidates { get; }
        bool[]? Charmask { get; }
        TeamExtendedInfo? TeamInfo { get; }
        HelpDto? Help { get; }
        Task EnsureLoadedAsync();
        Task RefreshAsync();
        void Invalidate();
        void Clear();
    }
}

using KvizCommando.Server.Services.PlayerCache;
using KvizCommando.Shared.Contracts.VsGame;

namespace KvizCommando.Server.Services.VsGame;

public sealed class VsGameService : IVsGameService
{
    private readonly IPlayerCacheService _cache;

    public VsGameService(IPlayerCacheService cache)
    {
        _cache = cache;
    }

    public Task<bool?> SaveBattleTeamAsync(
        int playerId,
        SaveBattleTeamRequest request,
        CancellationToken ct = default)
    {
        return _cache.UpdatePlayerLockedAsync(
            playerId,
            request.SessionId,
            player =>
            {
                var selectedSlots = request.SelectedSlotNumbers;

                if (selectedSlots.Length == 0 ||
                    !VsBattleClassificationRules.IsSupportedPartySize(
                        selectedSlots.Length) ||
                    selectedSlots.Any(slot => slot is < 1 or > 8) ||
                    selectedSlots.Distinct().Count() != selectedSlots.Length)
                {
                    return null;
                }

                var selectedMembers = selectedSlots
                    .Select(slot => player.Characters[slot - 1])
                    .ToArray();

                if (selectedMembers.Any(member =>
                        member is null || member.EnergyPoints <= 0))
                {
                    return null;
                }

                var eligibleIds =
                    VsBattleClassificationRules
                        .GetEligibleClassificationIds(
                            player.Core.RankEnum,
                            selectedMembers
                                .Select(member => member!.Rank)
                                .ToArray());

                if (eligibleIds.Length == 0)
                    return null;

                player.BattleTeamSlots = [.. selectedSlots];
                return DirtyFlags.None;
            },
            ct);
    }
}

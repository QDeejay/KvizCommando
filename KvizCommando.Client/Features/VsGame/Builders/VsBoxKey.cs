using KvizCommando.Client.Features.Home.Builders;

namespace KvizCommando.Client.Features.VsGame.Builders;

internal enum VsBoxKeyRoot
{
    CreateBattlefield = (int)HomeBoxKey.GameVs * 100 + 1,
    JoinBattlefield = (int)HomeBoxKey.GameVs * 100 + 2,
    RankedBattlefields = (int)HomeBoxKey.GameVs * 100 + 3
}

internal enum VsBoxKeyContent
{
    RankedBattleTeamManager,
    RankedMatchManager
}

internal enum VsBoxKeyRanked
{
    Classification = (int)HomeBoxKey.GameVs * 100 + 10
}

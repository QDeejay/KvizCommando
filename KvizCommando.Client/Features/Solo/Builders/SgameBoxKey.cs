using KvizCommando.Client.Features.Home.Builders;
using KvizCommando.Shared.Contracts.SoloGame;
using Microsoft.AspNetCore.Components;

namespace KvizCommando.Client.Features.Solo.Builders

{
    internal enum SgameBoxKeyRoot
    {
        Category = (int)HomeBoxKey.GameSolo * 100 + 20,
        Orientation = (int)HomeBoxKey.GameSolo * 100 + 50,
        Campaign = (int)HomeBoxKey.GameSolo * 100 + 3


    };

    internal enum SgameBoxKeySub
    {
        BtnCat,
        BtnOri,
        GameBoxCat,
        GameBoxOri
    }

    public sealed record SoloComponentParameters
    {
        public required SoloGameMode Mode { get; init; }
        public required int SelectionId { get; init; }
        public required string Title { get; init; }
        public required EventCallback<bool> OnGameCompletedChanged { get; init; }
        public required EventCallback<int> OnTeamLevelChanged { get; init; }
    }

}

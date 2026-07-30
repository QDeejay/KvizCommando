using KvizCommando.Shared.Contracts.VsGame.Match;
using KvizCommando.Shared.Models.Enums.VsGame;

namespace KvizCommando.Server.Services.VsGame.Match;

internal static class VsMatchSnapshotBuilder
{
    internal static (
        string ConnectionId,
        VsMatchSnapshot Snapshot)[] BuildMessages(
            VsMatchSession match) =>
        [
            .. match.Players
                .Where(player => player.IsConnected)
                .Select(player => (
                    player.ConnectionId,
                    BuildSnapshot(match, player)))
        ];

    private static VsMatchSnapshot BuildSnapshot(
        VsMatchSession match,
        VsMatchPlayerState currentPlayer) =>
        new()
        {
            MatchId = match.MatchId,
            ClassificationId =
                match.Classification.ClassificationId,
            Stake = match.Classification.Stake,
            Phase = match.Phase,
            DeadlineUtc = match.DeadlineUtc,
            PhaseDurationSeconds =
                match.Phase is
                    VsMatchPhase.PreparationOrder or
                    VsMatchPhase.PreparationCategories or
                    VsMatchPhase.PreparationHelps
                    ? match.Profile.PreparationSeconds
                    : 0,
            InfoKey = ResolveInfoKey(match, currentPlayer),
            Players =
            [
                .. match.Players.Select(player =>
                    new VsMatchPlayerDto
                    {
                        Position = player.Position,
                        DisplayName = player.DisplayName,
                        TeamName = player.TeamName,
                        TeamLevel = player.TeamLevel,
                        TeamPictureCode =
                            player.TeamPictureCode,
                        IsMe =
                            player.PlayerId ==
                            currentPlayer.PlayerId,
                        IsConnected = player.IsConnected,
                        IsFinished = player.IsFinished
                    })
            ],
            Preparation = BuildPreparation(
                match,
                currentPlayer)
        };

    private static VsPreparationDto BuildPreparation(
        VsMatchSession match,
        VsMatchPlayerState currentPlayer)
    {
        var assignedCharacterSlots = currentPlayer.Rounds
            .Where(round =>
                round.CharacterSlotNumber.HasValue)
            .Select(round =>
                round.CharacterSlotNumber!.Value)
            .ToHashSet();

        var assignedLoadoutPositions = currentPlayer.Rounds
            .Where(round => round.LoadoutPosition.HasValue)
            .Select(round => round.LoadoutPosition!.Value)
            .ToHashSet();

        return new VsPreparationDto
        {
            TeamSize = match.Classification.RequiredPartySize,
            IsFinished = currentPlayer.IsFinished,
            CanReset = VsMatchPreparationRules.CanReset(
                match.Phase,
                currentPlayer),
            CanFinish = !currentPlayer.IsFinished &&
                        VsMatchPreparationRules.CanFinish(
                            match.Phase,
                            currentPlayer),
            Rounds =
            [
                .. currentPlayer.Rounds.Select(round =>
                    BuildRound(currentPlayer, round))
            ],
            CharacterInventory =
            [
                .. currentPlayer.Characters
                    .Where(character =>
                        !assignedCharacterSlots.Contains(
                            character.SlotNumber))
                    .Select(ToCharacterDto)
            ],
            LoadoutInventory =
            [
                .. currentPlayer.Loadout
                    .Where(item =>
                        !assignedLoadoutPositions.Contains(
                            item.LoadoutPosition))
                    .OrderBy(item => item.LoadoutPosition)
                    .Select(ToLoadoutDto)
            ],
            HelpInventory =
            [
                .. Enum.GetValues<VsHelpType>()
                    .Where(help => help != VsHelpType.None)
                    .Select(help => new VsHelpCardDto
                    {
                        HelpType = help,
                        Count =
                            currentPlayer.HelpCounts[(int)help - 1] > 0 &&
                            currentPlayer.Rounds.All(round =>
                                round.HelpType != help)
                                ? 1
                                : 0
                    })
            ],
            CategoryModifiers = BuildCategoryModifiers(
                match,
                currentPlayer)
        };
    }

    private static VsPreparationRoundDto BuildRound(
        VsMatchPlayerState player,
        VsMatchRoundState round)
    {
        var character = round.CharacterSlotNumber.HasValue
            ? player.Characters.FirstOrDefault(item =>
                item.SlotNumber ==
                round.CharacterSlotNumber.Value)
            : null;

        var loadout = round.LoadoutPosition.HasValue
            ? player.Loadout.FirstOrDefault(item =>
                item.LoadoutPosition ==
                round.LoadoutPosition.Value)
            : null;

        return new VsPreparationRoundDto
        {
            RoundNumber = round.RoundNumber,
            IsCaptainRound = round.IsCaptainRound,
            Character = character is null
                ? null
                : ToCharacterDto(character),
            Loadout = loadout is null
                ? null
                : ToLoadoutDto(loadout),
            HelpType = round.HelpType
        };
    }

    private static VsCategoryModifierDto[] BuildCategoryModifiers(
        VsMatchSession match,
        VsMatchPlayerState currentPlayer)
    {
        var result = new List<VsCategoryModifierDto>();

        foreach (var round in currentPlayer.Rounds.Where(round =>
                     !round.IsCaptainRound))
        {
            for (var categoryId =
                     VsLoadoutCategoryIds.MinimumFactoryCategory;
                 categoryId <=
                     VsLoadoutCategoryIds.MaximumFactoryCategory;
                 categoryId++)
            {
                var seconds = 0d;

                foreach (var otherPlayer in match.Players.Where(player =>
                             player.PlayerId != currentPlayer.PlayerId))
                {
                    var otherRound =
                        otherPlayer.Rounds.First(item =>
                            item.RoundNumber == round.RoundNumber);

                    if (!otherRound.CharacterSlotNumber.HasValue)
                        continue;

                    var character =
                        otherPlayer.Characters.First(item =>
                            item.SlotNumber ==
                            otherRound.CharacterSlotNumber.Value);

                    if (character.CategoryModifiers.TryGetValue(
                            categoryId,
                            out var modifier))
                    {
                        seconds += modifier;
                    }
                }

                result.Add(new VsCategoryModifierDto
                {
                    RoundNumber = round.RoundNumber,
                    CategoryId = categoryId,
                    Seconds = Math.Truncate(seconds * 10) / 10
                });
            }
        }

        return [.. result];
    }

    private static VsCharacterCardDto ToCharacterDto(
        VsMatchCharacterState character) =>
        new()
        {
            SlotNumber = character.SlotNumber,
            Name = character.Name,
            PictureCode = character.PictureCode,
            Level = character.Level,
            OrientationId = character.OrientationId
        };

    private static VsLoadoutCardDto ToLoadoutDto(
        VsMatchLoadoutItemState item) =>
        new()
        {
            LoadoutPosition = item.LoadoutPosition,
            CategoryId = item.CategoryId,
            IsOwnQuestion = item.IsOwnQuestion,
            IsAllCategories = item.IsAllCategories,
            IsSelectable = !item.IsOwnQuestion
        };

    private static string ResolveInfoKey(
        VsMatchSession match,
        VsMatchPlayerState player)
    {
        if (player.IsFinished &&
            match.Phase is
                VsMatchPhase.PreparationOrder or
                VsMatchPhase.PreparationCategories or
                VsMatchPhase.PreparationHelps)
        {
            return "vsgame.Match.Info.WaitingForPlayers";
        }

        return match.Phase switch
        {
            VsMatchPhase.MatchLocked =>
                "vsgame.Match.Info.Locked",
            VsMatchPhase.PreparationOrder =>
                "vsgame.Match.Info.Order",
            VsMatchPhase.PreparationCategories =>
                "vsgame.Match.Info.Categories",
            VsMatchPhase.PreparationHelps =>
                "vsgame.Match.Info.Helps",
            VsMatchPhase.PreparationCompleted =>
                "vsgame.Match.Info.PreparationCompleted",
            _ => "vsgame.Match.Info.Aborted"
        };
    }
}

/**
 * MÓDOSÍTÁS: a snapshot a későbbi reklamációhoz megtartja a publikus
 * MatchId hivatkozást, technikai fázisverziót nem küld. A
 * loadoutkiosztást a stabil LoadoutPosition alapján építi.
 *
 * A szerveroldali meccsállapotból játékosonként
 * személyre szabott, tiszta SignalR-snapshotokat épít. Nem módosít
 * állapotot és nem küld hálózati üzenetet. A kategóriamódosító
 * lekérése explicit TryGetValue-t használ.
 */

using KvizCommando.Shared.Contracts.VsGame.Match;
using KvizCommando.Shared.Models.Enums.VsGame;

namespace KvizCommando.Server.Services.VsGame.Match;

internal static partial class VsMatchSnapshotBuilder
{
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
                     VsLoadoutCategoryIds.MINIMUM_FACTORY_CATEGORY;
                 categoryId <=
                     VsLoadoutCategoryIds.MAXIMUM_FACTORY_CATEGORY;
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
            EnergyPoints = character.EnergyPoints,
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
}

using KvizCommando.Shared.Models.Enums.VsGame;

namespace KvizCommando.Server.Services.VsGame.Match;

internal static class VsMatchPreparationRules
{
    internal static VsMatchRoundState? SelectCharacter(
        VsMatchSession match,
        VsMatchPlayerState? player,
        int slotNumber)
    {
        if (!CanModify(
                match,
                player,
                VsMatchPhase.PreparationOrder))
        {
            return null;
        }

        var currentPlayer = player!;

        if (currentPlayer.Characters.All(character =>
                character.SlotNumber != slotNumber) ||
            currentPlayer.Rounds.Any(round =>
                round.CharacterSlotNumber == slotNumber))
        {
            return null;
        }

        var target = currentPlayer.Rounds.FirstOrDefault(round =>
            !round.IsCaptainRound &&
            !round.CharacterSlotNumber.HasValue);

        if (target is null)
            return null;

        target.CharacterSlotNumber = slotNumber;
        return target;
    }

    internal static VsMatchRoundState? AssignLoadout(
        VsMatchSession match,
        VsMatchPlayerState? player,
        int loadoutPosition,
        int roundNumber)
    {
        if (!CanModify(
                match,
                player,
                VsMatchPhase.PreparationCategories))
        {
            return null;
        }

        var currentPlayer = player!;
        var loadout = currentPlayer.Loadout.FirstOrDefault(item =>
            item.LoadoutPosition == loadoutPosition);

        var target = currentPlayer.Rounds.FirstOrDefault(round =>
            !round.IsCaptainRound &&
            round.RoundNumber == roundNumber);

        if (loadout is null ||
            target is null ||
            loadout.IsOwnQuestion ||
            target.LoadoutPosition.HasValue ||
            currentPlayer.Rounds.Any(round =>
                round.LoadoutPosition == loadoutPosition))
        {
            return null;
        }

        target.LoadoutPosition = loadoutPosition;
        return target;
    }

    internal static VsMatchRoundState? AssignHelp(
        VsMatchSession match,
        VsMatchPlayerState? player,
        VsHelpType helpType,
        int roundNumber)
    {
        if (!CanModify(
                match,
                player,
                VsMatchPhase.PreparationHelps))
        {
            return null;
        }

        if (helpType is <= VsHelpType.None or >
            VsHelpType.AiSuggestion)
        {
            return null;
        }

        var currentPlayer = player!;
        var target = currentPlayer.Rounds.FirstOrDefault(round =>
            round.RoundNumber == roundNumber);

        if (target is null ||
            target.HelpType != VsHelpType.None ||
            !CanUseHelpInRound(helpType, target.IsCaptainRound) ||
            currentPlayer.HelpCounts[(int)helpType - 1] <= 0 ||
            currentPlayer.Rounds.Any(round =>
                round.HelpType == helpType))
        {
            return null;
        }

        target.HelpType = helpType;
        return target;
    }

    internal static bool Reset(
        VsMatchSession match,
        VsMatchPlayerState? player)
    {
        if (!CanModifyCurrentPhase(match, player))
            return false;

        var currentPlayer = player!;

        switch (match.Phase)
        {
            case VsMatchPhase.PreparationOrder:
                foreach (var round in currentPlayer.Rounds)
                    round.CharacterSlotNumber = null;
                return true;

            case VsMatchPhase.PreparationCategories:
                foreach (var round in currentPlayer.Rounds)
                    round.LoadoutPosition = null;
                return true;

            case VsMatchPhase.PreparationHelps:
                foreach (var round in currentPlayer.Rounds)
                    round.HelpType = VsHelpType.None;
                return true;

            default:
                return false;
        }
    }

    internal static bool Finish(
        VsMatchSession match,
        VsMatchPlayerState? player)
    {
        if (!CanModifyCurrentPhase(match, player))
            return false;

        var currentPlayer = player!;

        if (!CanFinish(match.Phase, currentPlayer))
            return false;

        currentPlayer.IsFinished = true;
        return true;
    }

    internal static bool CanFinish(
        VsMatchPhase phase,
        VsMatchPlayerState player) =>
        phase switch
        {
            VsMatchPhase.PreparationOrder =>
                player.Rounds
                    .Where(round => !round.IsCaptainRound)
                    .All(round =>
                        round.CharacterSlotNumber.HasValue),

            VsMatchPhase.PreparationCategories =>
                player.Rounds
                    .Where(round => !round.IsCaptainRound)
                    .All(round =>
                        round.LoadoutPosition.HasValue),

            VsMatchPhase.PreparationHelps => true,
            _ => false
        };

    internal static bool CanReset(
        VsMatchPhase phase,
        VsMatchPlayerState player) =>
        !player.IsFinished &&
        (phase switch
        {
            VsMatchPhase.PreparationOrder =>
                player.Rounds.Any(round =>
                    round.CharacterSlotNumber.HasValue),

            VsMatchPhase.PreparationCategories =>
                player.Rounds.Any(round =>
                    round.LoadoutPosition.HasValue),

            VsMatchPhase.PreparationHelps =>
                player.Rounds.Any(round =>
                    round.HelpType != VsHelpType.None),

            _ => false
        });

    internal static void BeginPhase(
        VsMatchSession match,
        VsMatchPhase phase)
    {
        foreach (var player in match.Players)
        {
            player.IsFinished =
                !player.IsConnected ||
                (phase == VsMatchPhase.PreparationHelps &&
                 player.HelpCounts.All(count => count <= 0));
        }
    }

    internal static VsMatchPhase? GetNextPhase(
        VsMatchSession match)
    {
        if (match.Players.Any(player => !player.IsFinished))
            return null;

        return match.Phase switch
        {
            VsMatchPhase.PreparationOrder =>
                VsMatchPhase.PreparationCategories,

            VsMatchPhase.PreparationCategories =>
                match.Players.Any(player =>
                    player.HelpCounts.Any(count => count > 0))
                    ? VsMatchPhase.PreparationHelps
                    : VsMatchPhase.PreparationCompleted,

            VsMatchPhase.PreparationHelps =>
                VsMatchPhase.PreparationCompleted,

            _ => null
        };
    }

    internal static void ApplyTimeoutDefaults(
        VsMatchSession match,
        VsMatchPlayerState player)
    {
        switch (match.Phase)
        {
            case VsMatchPhase.PreparationOrder:
            {
                var usedSlots = player.Rounds
                    .Where(round =>
                        round.CharacterSlotNumber.HasValue)
                    .Select(round =>
                        round.CharacterSlotNumber!.Value)
                    .ToHashSet();

                var remainingSlots = new Queue<int>(
                    player.Characters
                        .Where(character =>
                            !usedSlots.Contains(
                                character.SlotNumber))
                        .Select(character =>
                            character.SlotNumber));

                foreach (var round in player.Rounds.Where(round =>
                             !round.IsCaptainRound &&
                             !round.CharacterSlotNumber.HasValue))
                {
                    round.CharacterSlotNumber =
                        remainingSlots.Dequeue();
                }

                break;
            }

            case VsMatchPhase.PreparationCategories:
            {
                var usedPositions = player.Rounds
                    .Where(round =>
                        round.LoadoutPosition.HasValue)
                    .Select(round =>
                        round.LoadoutPosition!.Value)
                    .ToHashSet();

                var remainingPositions = new Queue<int>(
                    player.Loadout
                        .Where(item =>
                            !item.IsOwnQuestion &&
                            !usedPositions.Contains(
                                item.LoadoutPosition))
                        .OrderBy(item =>
                            item.LoadoutPosition)
                        .Select(item =>
                            item.LoadoutPosition));

                foreach (var round in player.Rounds.Where(round =>
                             !round.IsCaptainRound &&
                             !round.LoadoutPosition.HasValue))
                {
                    round.LoadoutPosition =
                        remainingPositions.Dequeue();
                }

                break;
            }
        }
    }

    private static bool CanModify(
        VsMatchSession match,
        VsMatchPlayerState? player,
        VsMatchPhase phase) =>
        match.Phase == phase &&
        CanModifyCurrentPhase(match, player);

    private static bool CanModifyCurrentPhase(
        VsMatchSession match,
        VsMatchPlayerState? player) =>
        !match.IsClosed &&
        player is not null &&
        player.IsConnected &&
        !player.IsFinished &&
        !HasExpired(match);

    private static bool CanUseHelpInRound(
        VsHelpType helpType,
        bool isCaptainRound) =>
        helpType switch
        {
            VsHelpType.FiftyFifty => true,
            VsHelpType.GuessRange => !isCaptainRound,
            VsHelpType.TimeFreeze => !isCaptainRound,
            VsHelpType.AiSuggestion => true,
            _ => false
        };

    private static bool HasExpired(VsMatchSession match) =>
        !match.Profile.PausePreparationOnTimeout &&
        match.DeadlineUtc.HasValue &&
        DateTime.UtcNow >= match.DeadlineUtc.Value;
}

/**
 * MÓDOSÍTÁS: a preparáció teljes szerveroldali szabályrendszere egy
 * helyre került. Az osztály ellenőrzi és a match lock alatt módosítja
 * a karakter-, kategória- és segítségkiosztást, a Reset/Finish
 * parancsokat, a timeout alapértékeit és a következő prep fázist.
 * SignalR-, adatbázis- és aszinkron függősége nincs.
 */

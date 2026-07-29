using KvizCommando.Shared.Models.Enums.VsGame;

namespace KvizCommando.Server.Services.VsGame.Match;

internal static class VsMatchPreparationRules
{
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
                    .All(round => round.LoadoutToken.HasValue),

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
                    round.LoadoutToken.HasValue),

            VsMatchPhase.PreparationHelps =>
                player.Rounds.Any(round =>
                    round.HelpType != VsHelpType.None),

            _ => false
        });
}

/**
 * ÚJ FÁJL: a preparáció Finish és Reset feltételeit egyetlen, tiszta
 * szabályhelyen tartja. Állapotot nem módosít, SignalR- és
 * adatbázisfüggősége nincs.
 */

using KvizCommando.Shared.Contracts.VsGame.Match;
using KvizCommando.Shared.Models.Enums.VsGame;

namespace KvizCommando.Server.Services.VsGame.Match;

internal static partial class VsMatchSnapshotBuilder
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
                ResolvePhaseDuration(match),
            InfoKey = ResolveInfoKey(match, currentPlayer),
            Players =
            [
                .. OrderPlayers(match).Select(player =>
                    new VsMatchPlayerDto
                    {
                        Position = player.Position,
                        DisplayName = player.IsBot
                            ? player.BotName
                            : player.DisplayName,
                        TeamName = player.TeamName,
                        TeamLevel = player.TeamLevel,
                        TeamPictureCode =
                            player.TeamPictureCode,
                        IsMe =
                            player.PlayerId ==
                            currentPlayer.PlayerId,
                        IsConnected = player.IsConnected,
                        IsBot = player.IsBot,
                        IsFinished = player.IsFinished,
                        TotalPoints = ResolveDisplayedPoints(
                            match,
                            player),
                        TotalTimeSeconds =
                            ResolveDisplayedTime(match, player),
                        ResponseTimeMilliseconds =
                            player.ResponseTimeMilliseconds,
                        ConnectionQuality =
                            player.ConnectionQuality,
                        ActiveCharacter =
                            BuildActiveCharacter(
                                match,
                                player)
                    })
            ],
            Preparation = BuildPreparation(
                match,
                currentPlayer),
            Game = BuildGame(match, currentPlayer),
            Reward = BuildReward(match, currentPlayer)
        };
    private static int ResolveDisplayedPoints(
        VsMatchSession match,
        VsMatchPlayerState player) =>
        match.Reward?.Players.FirstOrDefault(item =>
            item.PlayerId == player.PlayerId)?.FinalPoints ??
        player.TotalPoints;

    private static double ResolveDisplayedTime(
        VsMatchSession match,
        VsMatchPlayerState player) =>
        match.Reward?.Players.FirstOrDefault(item =>
            item.PlayerId == player.PlayerId)?.FinalTimeSeconds ??
        player.TotalTimeSeconds;
    private static VsCharacterCardDto? BuildActiveCharacter(
        VsMatchSession match,
        VsMatchPlayerState player)
    {
        if (match.Game.CurrentRoundNumber <= 0 ||
            match.Game.CurrentRoundNumber >
                match.Classification.RequiredPartySize ||
            match.Phase is
                VsMatchPhase.MatchLocked or
                VsMatchPhase.PreparationOrder or
                VsMatchPhase.PreparationCategories or
                VsMatchPhase.PreparationHelps or
                VsMatchPhase.GameStarting)
        {
            return null;
        }

        var round = player.Rounds.First(item =>
            item.RoundNumber ==
            match.Game.CurrentRoundNumber);

        if (!round.CharacterSlotNumber.HasValue)
            return null;

        var character = player.Characters.First(item =>
            item.SlotNumber ==
            round.CharacterSlotNumber.Value);

        return ToCharacterDto(character);
    }
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
            VsMatchPhase.PreparationStarting =>
                "vsgame.Match.Info.PreparationStarting",
            VsMatchPhase.PreparationOrder =>
                "vsgame.Match.Info.Order",
            VsMatchPhase.PreparationCategories =>
                "vsgame.Match.Info.Categories",
            VsMatchPhase.PreparationHelps =>
                "vsgame.Match.Info.Helps",
            VsMatchPhase.PreparationCompleted =>
                "vsgame.Match.Info.PreparationCompleted",
            VsMatchPhase.GameStarting =>
                "vsgame.Match.Info.GameStarting",
            VsMatchPhase.NormalRoundGuess =>
                "vsgame.Match.Info.Guess",
            VsMatchPhase.NormalRoundQuestion =>
                "vsgame.Match.Info.Question",
            VsMatchPhase.QuestionResult =>
                "vsgame.Match.Info.QuestionResult",
            VsMatchPhase.NormalRoundResult =>
                "vsgame.Match.Info.RoundResult",
            VsMatchPhase.CaptainQuestionSelection =>
                "vsgame.Match.Info.CaptainSelection",
            VsMatchPhase.CaptainQuestion =>
                "vsgame.Match.Info.CaptainQuestion",
            VsMatchPhase.CaptainRoundResult =>
                "vsgame.Match.Info.CaptainResult",
            VsMatchPhase.GameCompleted =>
                "vsgame.Match.Info.GameCompleted",
            _ => "vsgame.Match.Info.Aborted"
        };
    }

    private static IEnumerable<VsMatchPlayerState> OrderPlayers(
        VsMatchSession match)
    {
        if (match.Reward is not null)
        {
            return match.Reward.Players.Select(reward =>
                match.Players.First(player =>
                    player.PlayerId == reward.PlayerId));
        }

        return IsGamePhase(match.Phase)
            ? VsMatchScoring.OrderByStanding(match.Players)
            : match.Players.OrderBy(player => player.Position);
    }

    private static bool IsGamePhase(VsMatchPhase phase) =>
        phase is
            VsMatchPhase.GameStarting or
            VsMatchPhase.NormalRoundGuess or
            VsMatchPhase.NormalRoundQuestion or
            VsMatchPhase.QuestionResult or
            VsMatchPhase.NormalRoundResult or
            VsMatchPhase.CaptainQuestionSelection or
            VsMatchPhase.CaptainQuestion or
            VsMatchPhase.CaptainRoundResult or
            VsMatchPhase.GameCompleted;

    private static int ResolvePhaseDuration(
        VsMatchSession match) =>
        match.Phase switch
        {
            VsMatchPhase.PreparationOrder or
            VsMatchPhase.PreparationCategories or
            VsMatchPhase.PreparationHelps =>
                match.Profile.PreparationSeconds,
            VsMatchPhase.PreparationStarting or
            VsMatchPhase.GameStarting =>
                match.Profile.PhasePauseSeconds,
            VsMatchPhase.NormalRoundGuess =>
                match.Profile.GuessSeconds,
            VsMatchPhase.NormalRoundQuestion or
            VsMatchPhase.CaptainQuestion =>
                match.Profile.QuestionSeconds,
            VsMatchPhase.QuestionResult =>
                match.Profile.QuestionPauseSeconds,
            VsMatchPhase.NormalRoundResult or
            VsMatchPhase.CaptainRoundResult =>
                match.Profile.RoundResultSeconds,
            VsMatchPhase.CaptainQuestionSelection =>
                match.Profile.CaptainSelectionSeconds,
            _ => 0
        };

    private static bool IsAnswerPhase(
        VsMatchPhase phase) =>
        phase is
            VsMatchPhase.NormalRoundGuess or
            VsMatchPhase.NormalRoundQuestion or
            VsMatchPhase.CaptainQuestion;
}

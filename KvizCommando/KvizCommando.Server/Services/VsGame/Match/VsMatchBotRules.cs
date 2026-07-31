using KvizCommando.Shared.Models.Enums.VsGame;

namespace KvizCommando.Server.Services.VsGame.Match;

internal static class VsMatchBotRules
{
    internal static void Activate(
        VsMatchSession match,
        VsMatchPlayerState player)
    {
        player.IsConnected = false;
        player.IsBot = true;

        if (string.IsNullOrWhiteSpace(player.BotName))
            player.BotName = CreateName(match);
    }

    internal static bool SubmitAnswer(
        VsMatchSession match,
        VsMatchPlayerState player,
        DateTime receivedUtc)
    {
        if (!player.IsBot ||
            player.CurrentAnswer is not null ||
            IsLate(match, receivedUtc) ||
            match.Game.CurrentQuestion is not { } question)
        {
            return false;
        }

        var answerTime = Math.Max(
            0,
            (receivedUtc - match.PhaseStartedUtc).TotalSeconds);

        if (match.Phase == VsMatchPhase.NormalRoundGuess &&
            question.Kind == VsQuestionKind.Guess)
        {
            var upperLimit = Math.Max(
                10,
                Math.Abs(question.CorrectGuess) * 2);

            player.CurrentAnswer = new VsMatchPlayerAnswerState
            {
                QuestionNumber = match.Game.QuestionNumber,
                Guess = Math.Max(
                    1,
                    Math.Round(Random.Shared.NextDouble() * upperLimit)),
                AnswerTimeSeconds = answerTime
            };

            return true;
        }

        if (match.Phase is not
                (VsMatchPhase.NormalRoundQuestion or
                 VsMatchPhase.CaptainQuestion) ||
            question.Kind != VsQuestionKind.Choice)
        {
            return false;
        }

        player.CurrentAnswer = new VsMatchPlayerAnswerState
        {
            QuestionNumber = match.Game.QuestionNumber,
            AnswerIndex = Random.Shared.Next(0, 4),
            AnswerTimeSeconds = answerTime
        };

        return true;
    }

    private static bool IsLate(
        VsMatchSession match,
        DateTime receivedUtc) =>
        match.DeadlineUtc.HasValue &&
        receivedUtc > match.DeadlineUtc.Value;

    private static string CreateName(VsMatchSession match)
    {
        string name;

        do
        {
            name = $"CommandoBot{Random.Shared.Next(100, 1000)}";
        }
        while (match.Players.Any(player => player.BotName == name));

        return name;
    }
}

/**
 * ÚJ FÁJL: a kapcsolat nélküli bot tiszta domainműveletei. Beállítja
 * a botállapotot, és az aktuális szerverkérdésre véletlen tippet vagy
 * választ ad; SignalR-t, időzítőt, cache-t és adatbázist nem kezel.
 */

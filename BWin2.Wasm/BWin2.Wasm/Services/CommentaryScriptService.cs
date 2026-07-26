using BWin2.Wasm.Domain;

namespace BWin2.Wasm.Services;

internal sealed class CommentaryScriptService : ICommentaryScriptService
{
    private readonly IRandomSource _random;

    public CommentaryScriptService(IRandomSource random)
    {
        _random = random;
    }

    public IReadOnlyList<CommentaryPart> BuildMatchCommentary(
        GameState state,
        Fixture fixture,
        int scoringSide,
        int scorerNumber,
        int commentCode)
    {
        string script = state.Commentary.GetScript(commentCode);
        var parts = new List<CommentaryPart>(script.Length);

        foreach (char instruction in script)
        {
            int phraseCode = instruction - 64;
            string subject = ResolveSubject(
                state,
                fixture,
                scoringSide,
                scorerNumber,
                phraseCode);
            int distance = Qb.Int(_random.Next() * 18) + 15;
            string text = subject + " " + state.Commentary.GetPhrase(phraseCode) + " ";

            if (phraseCode == 3)
                text += Qb.Str(distance) + "meters... ";

            (int foreground, int background) = ResolveColors(state, fixture, scoringSide);
            parts.Add(new CommentaryPart(text, foreground, background));
        }

        return parts;
    }

    public IReadOnlyList<CommentaryPart> BuildPenaltyCommentary(
        GameState state,
        Fixture fixture,
        int scoringSide,
        int kickIndex,
        string script)
    {
        (int foreground, int background) = ResolveColors(state, fixture, scoringSide);

        if (script == "@")
        {
            return
            [
                new CommentaryPart(
                    "Draw. Penalty kicks decide today. ",
                    foreground,
                    background,
                    Colorize: false)
            ];
        }

        var parts = new List<CommentaryPart>(script.Length);
        foreach (char instruction in script)
        {
            int phraseCode = instruction - 64;
            string text;

            if (instruction == 'Z')
            {
                text = " Missed! ";
            }
            else
            {
                string subject = phraseCode is 3 or 4 or 11 or 17 or 20 or 21
                    ? " " + state.TeamAt(
                        scoringSide == 0
                            ? fixture.HomeTeamSlot
                            : fixture.AwayTeamSlot)
                        .Players[11 - kickIndex].Name
                    : string.Empty;
                text = subject + " " + state.Commentary.GetPhrase(phraseCode) + " ";
            }

            parts.Add(new CommentaryPart(text, foreground, background));
        }

        return parts;
    }

    private static string ResolveSubject(
        GameState state,
        Fixture fixture,
        int scoringSide,
        int scorerNumber,
        int phraseCode)
    {
        Team scoringTeam = state.TeamAt(
            scoringSide == 0 ? fixture.HomeTeamSlot : fixture.AwayTeamSlot);
        Team defendingTeam = state.TeamAt(
            scoringSide == 0 ? fixture.AwayTeamSlot : fixture.HomeTeamSlot);

        if (phraseCode is 1 or 2 or 5 or 6 or 7 or 8)
            return " " + scoringTeam.Name;
        if (phraseCode is 3 or 4 or 11 or 17 or 20 or 21)
            return " " + scoringTeam.Players[scorerNumber - 1].Name;
        if (phraseCode is 14 or 15)
            return " " + defendingTeam.Players[0].Name;
        return string.Empty;
    }

    private static (int Foreground, int Background) ResolveColors(
        GameState state,
        Fixture fixture,
        int scoringSide)
    {
        Team scoringTeam = state.TeamAt(
            scoringSide == 0 ? fixture.HomeTeamSlot : fixture.AwayTeamSlot);
        int foreground = scoringTeam.Stadium.ForegroundColor;
        int background = scoringTeam.Stadium.BackgroundColor;

        if (scoringSide == 1 &&
            background == state.TeamAt(fixture.HomeTeamSlot).Stadium.BackgroundColor)
        {
            (foreground, background) = (background, foreground);
        }

        return (foreground, background);
    }
}

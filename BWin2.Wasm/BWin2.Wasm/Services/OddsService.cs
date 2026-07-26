using BWin2.Wasm.Domain;

namespace BWin2.Wasm.Services;

internal sealed class OddsService : IOddsService
{
    public MatchOdds CalculateMatchOdds(
        Team homeTeam,
        Team awayTeam,
        bool cup,
        int cupRound)
    {
        double homeStrength = homeTeam.Strength + (cupRound >= 4 ? 0 : 10);
        double awayStrength = awayTeam.Strength;
        double homeDynamic = homeStrength;
        double awayDynamic = awayStrength;

        if (!cup)
        {
            homeDynamic += Qb.Int(
                homeTeam.Statistics.Wins * 3 +
                homeTeam.Statistics.Draws / 2d);
            awayDynamic += Qb.Int(
                awayTeam.Statistics.Wins * 3 +
                awayTeam.Statistics.Draws / 2d);
        }

        double difference = Math.Clamp(homeDynamic - awayDynamic, -48, 48);
        double home = 1.10001 + 1.7 + Qb.Int(difference * .709) * .05;
        double away = 1.10001 + 1.7 - Qb.Int(difference * .709) * .05;

        if (home > away)
            home = 2.11 + 1.8 / (away - 1);
        else if (home < away)
            away = 2.11 + 1.8 / (home - 1);

        double draw = 3.001 + Math.Abs(home - away) / 5;
        string drawText = Qb.Left(Qb.Str(draw), 5);

        if (cup)
        {
            home = (home - 1) / 2 + 1;
            away = (away - 1) / 2 + 1;
            drawText = "     ";
        }

        return new MatchOdds(
            HomeText: Qb.Left(Qb.Str(home), 5),
            DrawText: drawText,
            AwayText: Qb.Left(Qb.Str(away), 5));
    }

    public string CalculatePreseasonOdds(
        Team referenceTeam,
        Team selectedTeam,
        bool cupWinner)
    {
        double difference = referenceTeam.Strength - selectedTeam.Strength;
        double odds = difference * difference / 7 + 2.1001;

        if (cupWinner)
            odds = odds / 2 + 1.5;
        if (odds > 10)
            odds = Qb.Int(odds) + .001;

        return Qb.Left(Qb.Str(odds), 5);
    }
}

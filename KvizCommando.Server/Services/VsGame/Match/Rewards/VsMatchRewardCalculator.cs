using KvizCommando.Shared.Models;
using KvizCommando.Shared.Models.Enums.VsGame;
using KvizCommando.Shared.Models.Rules;

namespace KvizCommando.Server.Services.VsGame.Match;

internal static class VsMatchRewardCalculator
{
    internal const double BotFinalTimeSeconds = 9999;

    internal static VsMatchRewardState Calculate(VsMatchSession match)
    {
        var orderedPlayers = match.Players
            .OrderBy(player => player.IsBot)
            .ThenByDescending(player =>
                player.IsBot ? 0 : player.TotalPoints)
            .ThenBy(player =>
                player.IsBot
                    ? BotFinalTimeSeconds
                    : RoundToTenth(player.TotalTimeSeconds))
            .ThenBy(player => player.Position)
            .ToArray();
        var winner = orderedPlayers.FirstOrDefault(player => !player.IsBot);
        var prizePool = match.Classification.Stake * match.Players.Count;

        return new VsMatchRewardState
        {
            PrizePool = prizePool,
            Players =
            [
                .. orderedPlayers.Select((player, index) =>
                    BuildPlayerReward(
                        match,
                        player,
                        index + 1,
                        player == winner,
                        prizePool))
            ]
        };
    }

    private static VsMatchPlayerRewardState BuildPlayerReward(
        VsMatchSession match,
        VsMatchPlayerState player,
        int finalPosition,
        bool isWinner,
        int prizePool)
    {
        var consumedHelps = new int[4];

        foreach (var round in player.Rounds.Where(round =>
                     round.HelpType != VsHelpType.None))
        {
            consumedHelps[(int)round.HelpType - 1]++;
        }

        var credit = isWinner
            ? CalculateCreditReward(
                match.Classification.Stake,
                prizePool,
                player.TeamLevel)
            : new CreditReward();
        var characters = BuildCharacterRewards(
            player,
            isWinner ? prizePool / 8 : 0);
        var teamXp = CalculateTeamXp(
            match,
            player,
            characters);
        var winnerCompensation = isWinner
            ? VsRankedMatchRules.GetWinnerCompensation(
                player.TeamLevel)
            : VsRankedMatchRules.WINNER_COMPENSATION_MIN;
        var rankedScore = VsRankedMatchRules.GetRankedScore(
            player.TotalPoints,
            winnerCompensation);

        player.Statistics.Points = player.TotalPoints;
        player.Statistics.TimeSeconds = player.TotalTimeSeconds;

        return new VsMatchPlayerRewardState
        {
            PlayerId = player.PlayerId,
            SessionId = player.SessionId,
            OriginalPosition = player.Position,
            FinalPosition = finalPosition,
            DisplayName = player.IsBot ? player.BotName : player.DisplayName,
            TeamName = player.TeamName,
            TeamLevel = player.TeamLevel,
            IsBot = player.IsBot,
            IsWinner = isWinner,
            ActualPoints = player.TotalPoints,
            ActualTimeSeconds = player.TotalTimeSeconds,
            WinnerCompensation = winnerCompensation,
            RankedScore = rankedScore,
            FinalPoints = player.IsBot ? 0 : player.TotalPoints,
            FinalTimeSeconds = player.IsBot
                ? BotFinalTimeSeconds
                : player.TotalTimeSeconds,
            CharacterAverageXp = teamXp.CharacterAverage,
            ScoreXp = teamXp.Score,
            TeamXp = teamXp.Total,
            NewTeamLevel = CalculateNewTeamLevel(
                player,
                teamXp.Total),
            StakeReturn = credit.StakeReturn,
            BaseCreditReward = credit.BaseReward,
            TeamBonusCredit = credit.TeamBonus,
            TeamBonusPercent =
                RankRewards.List[player.TeamLevel].WinBonus,
            CreditReward = credit.Total,
            ConsumedHelps = consumedHelps,
            Statistics = player.Statistics,
            Characters = characters
        };
    }

    private static VsMatchCharacterRewardState[] BuildCharacterRewards(
        VsMatchPlayerState player,
        int pensionPool)
    {
        var rewards = player.Characters.Select(character =>
        {
            var total = player.CharacterRewardTotals.First(item =>
                item.SlotNumber == character.SlotNumber);
            var earnedXp = player.IsBot
                ? 0
                : Math.Max(total.CharacterXp, 0);
            var characterXp =
                TeamRules.GetCreditableMemberExperience(
                    earnedXp,
                    character.Level,
                    character.Xp);

            return new
            {
                Character = character,
                Total = total,
                EarnedXp = earnedXp,
                CharacterXp = characterXp
            };
        }).ToArray();
        var totalCharacterXp = rewards.Sum(item => item.CharacterXp);

        return
        [
            .. rewards.Select(item =>
                new VsMatchCharacterRewardState
                {
                    SlotNumber = item.Character.SlotNumber,
                    Name = item.Character.Name,
                    PictureCode = item.Character.PictureCode,
                    CharacterXp = item.CharacterXp,
                    IsCharacterXpCapped =
                        item.CharacterXp < item.EarnedXp,
                    EnergyLoss = item.Total.EnergyLoss,
                    Pension = totalCharacterXp == 0
                        ? 0
                        : (int)((long)pensionPool *
                            item.CharacterXp /
                            totalCharacterXp),
                    PlayDuels = item.Total.PlayDuels,
                    WinDuels = item.Total.WinDuels
                })
        ];
    }

    private static TeamXpReward CalculateTeamXp(
        VsMatchSession match,
        VsMatchPlayerState player,
        IReadOnlyCollection<VsMatchCharacterRewardState> characters)
    {
        if (player.IsBot || player.TeamLevel > 21)
            return new TeamXpReward();

        var averageCharacterXp = (int)Math.Floor(
            characters.Average(item =>
                item.CharacterXp));
        var rawScoreXp = (double)player.TotalPoints *
            match.Players.Count *
            player.TeamLevel /
            5;
        var calculatedScoreXp = rawScoreXp switch
        {
            > 0 => Math.Max((int)Math.Floor(rawScoreXp), 1),
            < 0 => Math.Min((int)Math.Floor(rawScoreXp), -1),
            _ => 0
        };
        var scoreXp = Math.Max(
            calculatedScoreXp,
            -averageCharacterXp);

        return new TeamXpReward(
            averageCharacterXp,
            scoreXp);
    }

    private static CreditReward CalculateCreditReward(
        int stake,
        int prizePool,
        int teamLevel)
    {
        var quarterPrize = prizePool / 4;
        var teamBonus = (int)Math.Floor(
            quarterPrize *
            RankRewards.List[teamLevel].WinBonus /
            100d);

        return new CreditReward(
            stake,
            quarterPrize,
            teamBonus);
    }

    private static int CalculateNewTeamLevel(
        VsMatchPlayerState player,
        int teamXp)
    {
        if (teamXp <= 0)
            return 0;

        var level = player.TeamLevel;
        var totalXp = player.TeamXp + teamXp;

        while (level <= TeamRules.LAST_XP_LEVEL &&
               totalXp >= RankRewards.List[level].NextLevelTeam)
        {
            level++;
        }

        return level > player.TeamLevel ? level : 0;
    }

    private static double RoundToTenth(double value) =>
        Math.Round(value, 1, MidpointRounding.AwayFromZero);

    private readonly record struct CreditReward(
        int StakeReturn = 0,
        int BaseReward = 0,
        int TeamBonus = 0)
    {
        internal int Total => StakeReturn + BaseReward + TeamBonus;
    }

    private readonly record struct TeamXpReward(
        int CharacterAverage = 0,
        int Score = 0)
    {
        internal int Total => CharacterAverage + Score;
    }
}

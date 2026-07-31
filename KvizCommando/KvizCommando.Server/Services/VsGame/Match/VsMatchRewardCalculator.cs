using KvizCommando.Shared.Models;
using KvizCommando.Shared.Models.Enums.VsGame;

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
        var pensionPerCharacter = winner is null
            ? 0
            : prizePool / 8 / winner.Characters.Length;

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
                        prizePool,
                        pensionPerCharacter))
            ]
        };
    }

    private static VsMatchPlayerRewardState BuildPlayerReward(
        VsMatchSession match,
        VsMatchPlayerState player,
        int finalPosition,
        bool isWinner,
        int prizePool,
        int pensionPerCharacter)
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
            FinalPoints = player.IsBot ? 0 : player.TotalPoints,
            FinalTimeSeconds = player.IsBot
                ? BotFinalTimeSeconds
                : player.TotalTimeSeconds,
            TeamXp = CalculateTeamXp(match, player),
            StakeReturn = credit.StakeReturn,
            BaseCreditReward = credit.BaseReward,
            TeamBonusCredit = credit.TeamBonus,
            TeamBonusPercent =
                RankRewards.List[player.TeamLevel].WinBonus,
            CreditReward = credit.Total,
            ConsumedHelps = consumedHelps,
            Statistics = player.Statistics,
            Characters =
            [
                .. player.Characters.Select(character =>
                    BuildCharacterReward(
                        player,
                        character,
                        isWinner,
                        pensionPerCharacter))
            ]
        };
    }

    private static VsMatchCharacterRewardState BuildCharacterReward(
        VsMatchPlayerState player,
        VsMatchCharacterState character,
        bool isWinner,
        int pensionPerCharacter)
    {
        var total = player.CharacterRewardTotals.First(item =>
            item.SlotNumber == character.SlotNumber);

        return new VsMatchCharacterRewardState
        {
            SlotNumber = character.SlotNumber,
            Name = character.Name,
            PictureCode = character.PictureCode,
            CharacterXp = player.IsBot ? 0 : total.CharacterXp,
            EnergyLoss = total.EnergyLoss,
            Pension = isWinner ? pensionPerCharacter : 0
        };
    }

    private static int CalculateTeamXp(
        VsMatchSession match,
        VsMatchPlayerState player)
    {
        if (player.IsBot || player.TeamLevel > 21)
            return 0;

        var baseXp = Math.Max(
            0,
            (int)Math.Floor(
                (double)player.TotalPoints *
                match.Players.Count *
                player.TeamLevel /
                10));
        var averageCharacterXp = (int)Math.Floor(
            player.CharacterRewardTotals.Average(item =>
                item.CharacterXp));

        return baseXp + averageCharacterXp;
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

    private static double RoundToTenth(double value) =>
        Math.Round(value, 1, MidpointRounding.AwayFromZero);

    private readonly record struct CreditReward(
        int StakeReturn = 0,
        int BaseReward = 0,
        int TeamBonus = 0)
    {
        internal int Total => StakeReturn + BaseReward + TeamBonus;
    }
}

/**
 * ÚJ FÁJL: tiszta, determinisztikus reward-kalkulátor. A bot
 * játék közben rendesen pontozódik, a végső jutalomsorrendben viszont
 * 0 ponttal és 9999 másodperccel az emberek mögé kerül, pozitív
 * jutalmat nem kap; a segítség- és energiavesztesége megmarad.
 * A 21-es szintig járó csapat-XP a korábbi pontképlet és a meccsben
 * szerzett karakter-XP-k lefelé kerekített átlagának összege; 21
 * fölött 0. A team bonus százaléka is a rewardállapot része.
 * PlayerCache-t és adatbázist nem érint.
 */

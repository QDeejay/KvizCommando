using KvizCommando.Shared.Contracts.VsGame.Match;
using KvizCommando.Shared.Models.Enums.VsGame;

namespace KvizCommando.Server.Services.VsGame.Match;

internal static partial class VsMatchSnapshotBuilder
{
    private static VsMatchRewardDto BuildReward(
        VsMatchSession match,
        VsMatchPlayerState currentPlayer)
    {
        if (match.Phase != VsMatchPhase.GameCompleted ||
            match.Reward is null)
        {
            return new VsMatchRewardDto();
        }

        var myReward = match.Reward.Players.First(player =>
            player.PlayerId == currentPlayer.PlayerId);

        return new VsMatchRewardDto
        {
            PrizePool = match.Reward.PrizePool,
            Standings =
            [
                .. match.Reward.Players.Select(player =>
                    new VsRewardStandingDto
                    {
                        FinalPosition = player.FinalPosition,
                        PlayerPosition = player.OriginalPosition,
                        DisplayName = player.DisplayName,
                        TeamName = player.TeamName,
                        TeamLevel = player.TeamLevel,
                        IsMe = player.PlayerId == currentPlayer.PlayerId,
                        IsBot = player.IsBot,
                        IsWinner = player.IsWinner,
                        Points = player.FinalPoints,
                        TimeSeconds = player.FinalTimeSeconds
                    })
            ],
            MyReward = new VsMyRewardDto
            {
                FinalPosition = myReward.FinalPosition,
                IsBot = myReward.IsBot,
                WinnerCompensation =
                    myReward.WinnerCompensation,
                RankedScore = myReward.RankedScore,
                IsTeamXpAvailable =
                    !myReward.IsBot && myReward.TeamLevel <= 21,
                CharacterAverageXp = myReward.CharacterAverageXp,
                ScoreXp = myReward.ScoreXp,
                TeamXp = myReward.TeamXp,
                NewTeamLevel = myReward.NewTeamLevel,
                StakeReturn = myReward.StakeReturn,
                BaseCreditReward = myReward.BaseCreditReward,
                TeamBonusCredit = myReward.TeamBonusCredit,
                TeamBonusPercent = myReward.TeamBonusPercent,
                CreditReward = myReward.CreditReward,
                ConsumedHelps = myReward.ConsumedHelps,
                Characters =
                [
                    .. myReward.Characters.Select(character =>
                        new VsCharacterRewardDto
                        {
                            SlotNumber = character.SlotNumber,
                            Name = character.Name,
                            PictureCode = character.PictureCode,
                            CharacterXp = character.CharacterXp,
                            IsCharacterXpCapped =
                                character.IsCharacterXpCapped,
                            EnergyLoss = character.EnergyLoss,
                            Pension = character.Pension
                        })
                ]
            }
        };
    }
}

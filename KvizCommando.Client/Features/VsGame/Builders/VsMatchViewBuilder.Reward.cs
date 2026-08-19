using KvizCommando.Client.Data;
using KvizCommando.Client.Features.VsGame.ViewModels;
using KvizCommando.Client.Helpers;
using KvizCommando.Client.Services.Visual.UiService.Language;
using KvizCommando.Shared.Contracts.VsGame.Match;
using KvizCommando.Shared.Models.Enums.VsGame;

namespace KvizCommando.Client.Features.VsGame.Builders;

partial class VsMatchViewBuilder
{
    private VsMatchRewardViewData BuildReward(VsMatchRewardDto data)
    {
        var myReward = data.MyReward;

        return new VsMatchRewardViewData
        {
            PrizePool = data.PrizePool,
            Standings =
            [
                .. data.Standings.Select(item =>
                    new VsRewardStandingVm
                    {
                        FinalPosition = item.FinalPosition,
                        PlayerPosition = item.PlayerPosition,
                        DisplayName = item.DisplayName,
                        TeamName = item.TeamName,
                        TeamLevel = RankNameTable.Data[item.TeamLevel]
                            .PublicLevel ?? string.Empty,
                        IsMe = item.IsMe,
                        IsBot = item.IsBot,
                        IsWinner = item.IsWinner,
                        Points = item.Points,
                        TimeSeconds = item.TimeSeconds
                    })
            ],
            MyReward = myReward is null
                ? null
                : new VsMyRewardVm
                {
                    FinalPosition = myReward.FinalPosition,
                    WinnerCompensation =
                        myReward.WinnerCompensation,
                    RankedScore = myReward.RankedScore,
                    IsTeamXpAvailable =
                        myReward.IsTeamXpAvailable,
                    CharacterAverageXp =
                        myReward.CharacterAverageXp,
                    ScoreXp = myReward.ScoreXp,
                    TeamXp = myReward.TeamXp,
                    NewTeamLevel = myReward.NewTeamLevel,
                    StakeReturn = myReward.StakeReturn,
                    BaseCreditReward = myReward.BaseCreditReward,
                    TeamBonusCredit = myReward.TeamBonusCredit,
                    TeamBonusPercent =
                        myReward.TeamBonusPercent,
                    CreditReward = myReward.CreditReward,
                    ConsumedHelps =
                    [
                        .. myReward.ConsumedHelps
                            .Select((count, index) =>
                                new VsConsumedHelpVm
                                {
                                    Name = _lang[
                                        $"vsgame.Match.Help.{index + 1}"],
                                    Count = count
                                })
                            .Where(item => item.Count > 0)
                    ],
                    Characters =
                    [
                        .. myReward.Characters.Select(character =>
                            new VsCharacterRewardVm
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

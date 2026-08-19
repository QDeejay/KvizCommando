using KvizCommando.Client.Data;
using KvizCommando.Client.Features.VsGame.ViewModels;
using KvizCommando.Client.Helpers;
using KvizCommando.Client.Services.Visual.UiService.Language;
using KvizCommando.Shared.Contracts.VsGame.Match;
using KvizCommando.Shared.Models.Enums.VsGame;

namespace KvizCommando.Client.Features.VsGame.Builders;

partial class VsMatchViewBuilder
{
    private VsPreparationViewData BuildPreparation(
        VsPreparationDto data,
        string culture)
    {
        return new VsPreparationViewData
        {
            IsFinished = data.IsFinished,
            CanReset = data.CanReset,
            CanFinish = data.CanFinish,
            Rounds =
            [
                .. data.Rounds.Select(round =>
                    BuildRound(round, culture))
            ],
            Characters =
            [
                .. data.CharacterInventory.Select(character =>
                    BuildCharacter(character, culture))
            ],
            Loadout =
            [
                .. data.LoadoutInventory.Select(loadout =>
                    BuildLoadout(loadout, culture))
            ],
            Helps =
            [
                .. data.HelpInventory.Select(BuildHelp)
            ],
            CategoryModifiers =
            [
                .. data.CategoryModifiers.Select(item =>
                    new VsCategoryModifierVm
                    {
                        RoundNumber = item.RoundNumber,
                        CategoryId = item.CategoryId,
                        Seconds = item.Seconds
                    })
            ]
        };
    }

    private VsPreparationRoundVm BuildRound(
        VsPreparationRoundDto round,
        string culture)
    {
        return new VsPreparationRoundVm
        {
            RoundNumber = round.RoundNumber,
            RoundText = round.IsCaptainRound
                ? _lang["vsgame.Match.Round.Captain"]
                : _lang["vsgame.Match.Round.Normal"]
                    .FormatSafe(round.RoundNumber),
            IsCaptainRound = round.IsCaptainRound,
            Character = round.Character is null
                ? null
                : BuildCharacter(round.Character, culture),
            Loadout = round.Loadout is null
                ? null
                : BuildLoadout(round.Loadout, culture),
            Help = round.HelpType == VsHelpType.None
                ? null
                : BuildHelp(new VsHelpCardDto
                {
                    HelpType = round.HelpType,
                    Count = 1
                })
        };
    }
}

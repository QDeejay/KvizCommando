using KvizCommando.Client.Data;
using KvizCommando.Client.Features.VsGame.Match.ViewModels;
using KvizCommando.Client.Helpers;
using KvizCommando.Client.Services.Visual.UiService.Language;
using KvizCommando.Shared.Contracts.VsGame.Match;
using KvizCommando.Shared.Models.Enums.VsGame;

namespace KvizCommando.Client.Features.VsGame.Match.Builders;

public sealed class VsMatchViewBuilder
{
    private const string CATEGORY_IMAGE_ROOT =
        "images/buttons/solo/categories";
    private const string ORIENTATION_IMAGE_ROOT =
        "images/buttons/solo/orients";

    private static readonly string[] CategoryFileNames =
    [
        "",
        "religion",
        "famousdates",
        "music",
        "sport",
        "technology",
        "naturalscience",
        "famouspepole",
        "sculpture_painting",
        "mythology",
        "history",
        "movies",
        "game",
        "it",
        "geo_astro",
        "fashion",
        "literature"
    ];

    private static readonly string[] OrientationFileNames =
    [
        "",
        "teologist",
        "historian",
        "artist",
        "gamer",
        "engineer",
        "scientist",
        "trendy",
        "educated"
    ];

    private readonly ILanguageService _lang;

    public VsMatchViewBuilder(ILanguageService lang)
    {
        _lang = lang;
    }

    public VsQueueViewData BuildQueue(
        VsRankedQueueSnapshot snapshot)
    {
        return new VsQueueViewData
        {
            ClassificationText =
                _lang[
                    $"vsgame.Classification.Title.{snapshot.ClassificationId}"],
            StatusText = _lang["vsgame.Match.Queue.Status"],
            WaitingPlayers = snapshot.WaitingPlayers,
            RequiredPlayers = snapshot.RequiredPlayers,
            RequiredPartySize = snapshot.RequiredPartySize,
            Stake = snapshot.Stake,
            Players =
            [
                .. snapshot.Players.Select(BuildPlayer)
            ]
        };
    }

    public VsMatchViewData Build(
        VsMatchSnapshot snapshot,
        string culture)
    {
        return new VsMatchViewData
        {
            MatchId = snapshot.MatchId,
            Phase = snapshot.Phase,
            DeadlineUtc = snapshot.DeadlineUtc,
            PhaseDurationSeconds =
                snapshot.PhaseDurationSeconds,
            InfoText = _lang[snapshot.InfoKey],
            ClassificationText =
                _lang[
                    $"vsgame.Classification.Title.{snapshot.ClassificationId}"],
            Stake = snapshot.Stake,
            Players =
            [
                .. snapshot.Players.Select(player =>
                    BuildPlayer(player))
            ],
            Preparation = BuildPreparation(
                snapshot.Preparation,
                culture)
        };
    }

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

    private VsRosterPlayerVm BuildPlayer(
        VsMatchPlayerDto player)
    {
        var rankIndex = Math.Clamp(
            player.TeamLevel,
            0,
            RankNameTable.Data.Count - 1);

        return new VsRosterPlayerVm
        {
            Position = player.Position,
            DisplayName = player.DisplayName,
            TeamName = player.TeamName,
            TeamLevel =
                RankNameTable.Data[rankIndex].PublicLevel ??
                string.Empty,
            TeamPictureSrc =
                "images/avatars/basic.webp",
            IsMe = player.IsMe,
            IsConnected = player.IsConnected,
            IsFinished = player.IsFinished
        };
    }

    private static VsCharacterCardVm BuildCharacter(
        VsCharacterCardDto character,
        string culture)
    {
        var levelIndex = Math.Clamp(
            character.Level,
            0,
            RankNameTable.Data.Count - 1);

        var orientationId = Math.Clamp(
            character.OrientationId,
            1,
            OrientationFileNames.Length - 1);

        return new VsCharacterCardVm
        {
            SlotNumber = character.SlotNumber,
            Name = character.Name,
            PictureCode = character.PictureCode,
            LevelText =
                RankNameTable.Data[levelIndex].PublicLevel ??
                string.Empty,
            OrientationName =
                OrientationLocalizer.GetOrientation(
                    orientationId,
                    culture),
            OrientationImageSrc =
                $"{ORIENTATION_IMAGE_ROOT}/" +
                $"{OrientationFileNames[orientationId]}.webp"
        };
    }

    private VsLoadoutCardVm BuildLoadout(
        VsLoadoutCardDto loadout,
        string culture)
    {
        return new VsLoadoutCardVm
        {
            LoadoutToken = loadout.LoadoutToken,
            LoadoutPosition = loadout.LoadoutPosition,
            CategoryId = loadout.CategoryId,
            CategoryName = ResolveCategoryName(
                loadout.CategoryId,
                culture),
            ImageSrc = ResolveCategoryImage(
                loadout.CategoryId),
            IsOwnQuestion = loadout.IsOwnQuestion,
            IsSelectable = loadout.IsSelectable
        };
    }

    private VsHelpCardVm BuildHelp(VsHelpCardDto help)
    {
        return new VsHelpCardVm
        {
            HelpType = help.HelpType,
            Name =
                _lang[
                    $"vsgame.Match.Help.{(int)help.HelpType}"],
            IconCss = help.HelpType switch
            {
                VsHelpType.FiftyFifty =>
                    "bi bi-circle-half",
                VsHelpType.GuessRange =>
                    "bi bi-arrows-expand",
                VsHelpType.TimeFreeze =>
                    "bi bi-hourglass-split",
                VsHelpType.AiSuggestion =>
                    "bi bi-cpu",
                _ => "bi bi-question"
            },
            Count = help.Count
        };
    }

    private string ResolveCategoryName(
        int categoryId,
        string culture)
    {
        return categoryId switch
        {
            VsLoadoutCategoryIds.OwnQuestion =>
                _lang["vsgame.Match.Category.Own"],
            VsLoadoutCategoryIds.AllCategories =>
                _lang["vsgame.Match.Category.All"],
            _ => CategoryNameLocalizer.GetCategory(
                categoryId,
                culture)
        };
    }

    private static string ResolveCategoryImage(int categoryId)
    {
        if (categoryId == VsLoadoutCategoryIds.OwnQuestion)
            return "images/buttons/question/usr.webp";

        if (categoryId == VsLoadoutCategoryIds.AllCategories)
            return "images/buttons/solo/categories.webp";

        return categoryId >= 1 &&
               categoryId < CategoryFileNames.Length
            ? $"{CATEGORY_IMAGE_ROOT}/" +
              $"{CategoryFileNames[categoryId]}.webp"
            : string.Empty;
    }
}

/**
 * MÓDOSÍTÁS: a queue publikus játékosadataiból is ugyanazzal a
 * BuildPlayer leképezéssel készít lobby rostert.
 *
 * A szerver snapshotjából lokalizált neveket, meglévő Solo
 * kategória-/orientációképeket és VS megjelenítési view modelleket
 * épít. DI-be nem kerül, kizárólag a manager példányosítja.
 */

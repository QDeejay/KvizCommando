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
                .. snapshot.Players.Select(player =>
                    BuildPlayer(player))
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
                    BuildPlayer(player, culture))
            ],
            Preparation = BuildPreparation(
                snapshot.Preparation,
                culture),
            Game = BuildGame(snapshot.Game, culture)
        };
    }

    private VsGameViewData BuildGame(
        VsGameDto data,
        string culture) =>
        new()
        {
            CurrentRoundNumber = data.CurrentRoundNumber,
            NormalRoundCount = data.NormalRoundCount,
            QuestionNumber = data.QuestionNumber,
            QuestionKind = data.QuestionKind,
            QuestionerPosition = data.QuestionerPosition,
            Question = data.Question,
            Answers = data.Answers,
            CorrectAnswerIndex = data.CorrectAnswerIndex,
            CorrectGuess = data.CorrectGuess,
            MyAnswerIndex = data.MyAnswerIndex,
            MyGuess = data.MyGuess,
            MyTimeModifierSeconds =
                data.MyTimeModifierSeconds,
            MyRoundPoints = data.MyRoundPoints,
            MyRoundTimeSeconds =
                data.MyRoundTimeSeconds,
            CanAnswer = data.CanAnswer,
            CanChooseCaptainQuestion =
                data.CanChooseCaptainQuestion,
            QuestionPlayers =
            [
                .. data.QuestionPlayers.Select(item =>
                    new VsQuestionPlayerVm
                    {
                        Position = item.Position,
                        HasAnswered = item.HasAnswered,
                        AnswerIndex = item.AnswerIndex,
                        Guess = item.Guess,
                        IsCorrect = item.IsCorrect,
                        AnswerTimeSeconds =
                            item.AnswerTimeSeconds,
                        ModifiedTimeSeconds =
                            item.ModifiedTimeSeconds,
                        Points = item.Points,
                        HasSpeedBonus =
                            item.HasSpeedBonus
                    })
            ],
            Progress =
            [
                .. data.Progress.Select(item =>
                    new VsRoundProgressVm
                    {
                        StepNumber = item.StepNumber,
                        PlayerPosition =
                            item.PlayerPosition,
                        IsGuess = item.IsGuess,
                        IsCompleted = item.IsCompleted,
                        IsCurrent = item.IsCurrent,
                        Points = item.Points
                    })
            ],
            RoundResult =
            [
                .. data.RoundResult.Select(item =>
                    new VsRoundResultVm
                    {
                        Position = item.Position,
                        TotalBefore = item.TotalBefore,
                        RoundPoints = item.RoundPoints,
                        TotalAfter = item.TotalAfter,
                        RoundTimeSeconds =
                            item.RoundTimeSeconds,
                        HasWinnerBonus =
                            item.HasWinnerBonus,
                        HasFastestBonus =
                            item.HasFastestBonus,
                        CharacterSlotNumber =
                            item.CharacterSlotNumber,
                        CharacterXp = item.CharacterXp,
                        EnergyLoss = item.EnergyLoss
                    })
            ],
            CaptainQuestions =
            [
                .. data.CaptainQuestions.Select(item =>
                    new VsCaptainQuestionVm
                    {
                        LoadoutPosition =
                            item.LoadoutPosition,
                        CategoryId = item.CategoryId,
                        CategoryName =
                            ResolveCategoryName(
                                item.CategoryId,
                                culture),
                        ImageSrc =
                            ResolveCategoryImage(
                                item.CategoryId),
                        Question = item.Question
                    })
            ],
            CaptainOrder = data.CaptainOrder,
            CaptainOrderIndex = data.CaptainOrderIndex
        };

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
        VsMatchPlayerDto player,
        string? culture = null)
    {
        return new VsRosterPlayerVm
        {
            Position = player.Position,
            DisplayName = player.DisplayName,
            TeamName = player.TeamName,
            TeamLevel =
                RankNameTable.Data[player.TeamLevel].PublicLevel ??
                string.Empty,
            TeamPictureSrc =
                "images/avatars/basic.webp",
            IsMe = player.IsMe,
            IsConnected = player.IsConnected,
            IsFinished = player.IsFinished,
            TotalPoints = player.TotalPoints,
            TotalTimeSeconds = player.TotalTimeSeconds,
            ActiveCharacter =
                player.ActiveCharacter is null ||
                string.IsNullOrWhiteSpace(culture)
                    ? null
                    : BuildCharacter(
                        player.ActiveCharacter,
                        culture)
        };
    }

    private static VsCharacterCardVm BuildCharacter(
        VsCharacterCardDto character,
        string culture)
    {
        return new VsCharacterCardVm
        {
            SlotNumber = character.SlotNumber,
            Name = character.Name,
            PictureCode = character.PictureCode,
            LevelText =
                RankNameTable.Data[character.Level].PublicLevel ??
                string.Empty,
            OrientationName =
                OrientationLocalizer.GetOrientation(
                    character.OrientationId,
                    culture)
        };
    }

    private VsLoadoutCardVm BuildLoadout(
        VsLoadoutCardDto loadout,
        string culture)
    {
        return new VsLoadoutCardVm
        {
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
 * MÓDOSÍTÁS: a meccsazonosító a későbbi reklamációhoz megmaradt, a
 * LoadoutPosition mellett felesleges loadout-token leképezése
 * megszűnt. A queue publikus játékosadataiból továbbra is ugyanazzal a
 * BuildPlayer leképezéssel készít lobby rostert.
 *
 * MÓDOSÍTÁS: a gameplay snapshotot is megjelenítési modellekké
 * alakítja; a kapitányi kérdésekhez ugyanazt a meglévő kategória-
 * kép- és névfeloldást használja.
 * A szerveren számolt saját időmódosítót változtatás nélkül viszi át.
 * MÓDOSÍTÁS: a garantáltan érvényes csapat- és karakterszinteket,
 * illetve orientációt közvetlenül használja; nem clampeli őket.
 * A karakter view modelből kikerült a sehol nem használt orientációs
 * képútvonal, mert a kártyákon a CharacterView SVG jelenik meg.
 *
 * A szerver snapshotjából lokalizált neveket, meglévő Solo képeket
 * és VS view modelleket épít. DI-be nem kerül.
 */

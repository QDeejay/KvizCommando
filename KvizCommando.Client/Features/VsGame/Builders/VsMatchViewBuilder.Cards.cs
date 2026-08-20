using KvizCommando.Client.Data;
using KvizCommando.Client.Features.VsGame.ViewModels;
using KvizCommando.Client.Helpers;
using KvizCommando.Client.Services.Visual.UiService.Language;
using KvizCommando.Shared.Contracts.VsGame.Match;
using KvizCommando.Shared.Models.Enums.VsGame;
using KvizCommando.Shared.Models.Rules;

namespace KvizCommando.Client.Features.VsGame.Builders;

partial class VsMatchViewBuilder
{
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
            TeamPictureSrc = AvatarImageSrc(player.TeamPictureCode),
            IsMe = player.IsMe,
            IsConnected = player.IsConnected,
            IsBot = player.IsBot,
            IsFinished = player.IsFinished,
            TotalPoints = player.TotalPoints,
            TotalTimeSeconds = player.TotalTimeSeconds,
            ResponseTimeMilliseconds =
                player.ResponseTimeMilliseconds,
            ConnectionQuality =
                player.ConnectionQuality,
            ActiveCharacter =
                player.ActiveCharacter is null ||
                string.IsNullOrWhiteSpace(culture)
                    ? null
                    : BuildCharacter(
                        player.ActiveCharacter,
                        culture)
        };
    }

    private static string AvatarImageSrc(string? avatar) =>
        ProfileRules.TryGetAvatarNumber(avatar, out var avatarNumber)
            ? $"images/avatars/avatar-{avatarNumber:D2}.webp"
            : $"images/avatars/avatar-{ProfileRules.DEFAULT_AVATAR_NO:D2}.webp";

    private static VsCharacterCardVm BuildCharacter(
        VsCharacterCardDto character,
        string culture)
    {
        return new VsCharacterCardVm
        {
            SlotNumber = character.SlotNumber,
            Name = character.Name,
            PictureCode = character.PictureCode,
            Level = character.Level,
            EnergyPoints = character.EnergyPoints,
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
            VsLoadoutCategoryIds.OWN_QUESTION =>
                _lang["vsgame.Match.Category.Own"],
            VsLoadoutCategoryIds.ALL_CATEGORIES =>
                _lang["vsgame.Match.Category.All"],
            _ => CategoryNameLocalizer.GetCategory(
                categoryId,
                culture)
        };
    }

    private static string ResolveCategoryImage(int categoryId)
    {
        if (categoryId == VsLoadoutCategoryIds.OWN_QUESTION)
            return "images/buttons/question/usr.webp";

        if (categoryId == VsLoadoutCategoryIds.ALL_CATEGORIES)
            return "images/buttons/solo/categories.webp";

        return categoryId >= 1 &&
               categoryId < CategoryFileNames.Length
            ? $"{CATEGORY_IMAGE_ROOT}/" +
              $"{CategoryFileNames[categoryId]}.webp"
            : string.Empty;
    }
}

using KvizCommando.Client.Features.VsGame.Components;
using KvizCommando.Client.Helpers;
using KvizCommando.Client.Models.ViewModels;
using KvizCommando.Client.Services.Visual.UiService.Language;
using KvizCommando.Shared.Models.Dtos;

namespace KvizCommando.Client.Features.VsGame.Builders;

public sealed class VsComponentParameters
{
    public Func<Task>? OnTeamSaved { get; init; }
}

public static class VsBoxBuilder
{
    public static readonly string[] Root =
        Enum.GetNames<VsBoxKeyRoot>();

    public static readonly string[] Ranked =
    [
        VsBoxKeyContent.RankedBattleTeamManager.ToString(),
        .. BuildClassificationNames()
    ];

    public static Dictionary<string, ContentBoxVm> BuildBoxes(
        VsGameDtos data,
        VsComponentParameters parameters,
        ILanguageService lang)
    {
        var boxes = new Dictionary<string, ContentBoxVm>();

        foreach (var spec in VsGameBoxSpecs.Specs)
        {
            var key = spec.Key.ToString();
            var isManager =
                spec.Key.Equals(
                    VsBoxKeyContent.RankedBattleTeamManager);
            var enabled = spec.CheckEnable(data.RootBoxInfo);

            boxes.Add(key, new ContentBoxVm
            {
                DictKey = key,
                Header = lang[spec.TitleKey],
                Footer = isManager
                    ? lang["vsgame.Manager.Footer.Toggle"]
                    : spec.FooterDisplay
                        ? spec.BuildFooter(lang, data.RootBoxInfo)
                        : string.Empty,
                FooterDisplay = spec.FooterDisplay,
                Size = isManager
                    ? ResolveManagerSize(
                        data.RankedBattlefields
                            .SavedSelection.SelectedSlotNumbers)
                    : spec.Size,
                ReSizable = isManager,
                ImageSrc = spec.ImageSrc,
                BgImageSrc = spec.BgImageSrc,
                IsEnabled = enabled,
                IsClickable = enabled && spec.ClickId > 0,
                ClickId = spec.ClickId,
                LcdDisplay = spec.LcdBackground,
                RenderContent = spec.RenderContent,
                BodyComponent = spec.BodyComp,
                BodyParameters = isManager
                    ? new Dictionary<string, object?>
                    {
                        [nameof(
                            RankedBattleTeamManager.OnTeamSaved)] =
                            parameters.OnTeamSaved
                    }
                    : []
            });
        }

        foreach (var classification in
                 data.RankedBattlefields.Classifications)
        {
            var id = classification.ClassificationId;
            var key = BuildClassificationKey(id);
            var enabled = data.RankedBattlefields
                .SavedSelection
                .EligibleClassificationIds
                .Contains(id);

            boxes.Add(key, new ContentBoxVm
            {
                DictKey = key,
                Header = lang[
                    $"vsgame.Classification.Title.{id}"],
                Footer = lang[
                    "vsgame.Classification.Footer.Requirements"]
                    .FormatSafe(
                        classification.RequiredPartySize,
                        classification.MemberMinimumRankClass,
                        classification.MemberMaximumRankClass),
                FooterDisplay = true,
                Size = ContentBoxSize.BUTTON_WIDE,
                IsEnabled = enabled,
                IsClickable = enabled,
                ClickId = 610 + id
            });
        }

        return boxes;
    }

    private static string ResolveManagerSize(int[] slots) =>
        slots.Length > 0 && slots.All(slot => slot > 0)
            ? ContentBoxSize.MINIMALIZED
            : ContentBoxSize.CONTENT_LARGE;

    private static string[] BuildClassificationNames()
    {
        var count = (int)VsBoxKeyRanked.Classification;
        var names = new string[count];

        for (var i = 0; i < count; i++)
            names[i] = BuildClassificationKey(i + 1);

        return names;
    }

    private static string BuildClassificationKey(int id) =>
        $"{VsBoxKeyRanked.Classification}{id}";
}

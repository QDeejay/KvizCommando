using KvizCommando.Client.Models.ViewModels;
using KvizCommando.Client.Services.Visual.UiService.Language;
using KvizCommando.Shared.Models.Dtos;
using Microsoft.AspNetCore.Components;

namespace KvizCommando.Client.Features.VsGame.Builders;


public static class VsBoxBuilder
{
    public static readonly string[] Root = Enum.GetNames<VsBoxKeyRoot>();

    public static readonly string[] Ranked =
    [
        VsBoxKeyContent.RankedBattleTeamManager.ToString(),
        .. BuildClassificationNames()
    ];

    public static readonly string[] Match =
    [
        VsBoxKeyContent.RankedMatchManager.ToString()
    ];

    public static Dictionary<string, ContentBoxVm> BuildBoxes(
        VsGameDtos data,
        VsComponentParameters parameters,
        ILanguageService lang)
    {
        var boxes = new Dictionary<string, ContentBoxVm>(
            VsGameBoxSpecs.Specs.Count +
            data.RankedBattlefields.Classifications.Length);

        foreach (var spec in VsGameBoxSpecs.Specs)
        {
            var key = spec.Key.ToString();
            var enabled = spec.CheckEnable(data, 0);

            boxes.Add(key, new ContentBoxVm
            {
                DictKey = key,
                Header = lang[spec.TitleKey],
                Footer = spec.FooterDisplay ? spec.BuildFooter(lang, data, 0) : string.Empty,
                FooterDisplay = spec.FooterDisplay,
                Size = spec.ReSizable ? spec.SizeBuilder(data) : spec.Size,
                ReSizable = spec.ReSizable,
                ImageSrc = spec.ImageSrc,
                BgImageSrc = spec.BgImageSrc,
                IsEnabled = enabled,
                IsClickable = enabled && spec.ClickId > 0,
                ClickId = spec.ClickId,
                LcdDisplay = spec.LcdBackground,
                RenderContent = spec.RenderContent,
                BodyComponent = spec.BodyComp,
                BodyParameters = spec.BodyComp is null ? [] : spec.BuildParams(parameters)
            });
        }

        foreach (var spec in VsGameBoxSpecs.SubSpecs)
        {
            foreach (var classification in
                     data.RankedBattlefields.Classifications)
            {
                var id = classification.ClassificationId;
                var key = $"{spec.Key}{id}";

                boxes.Add(key, new ContentBoxVm
                {
                    DictKey = key,
                    Header = spec.BuildTitle(lang, id),
                    Footer = spec.FooterDisplay ? spec.BuildFooter(lang, data, id) : string.Empty,
                    FooterDisplay = spec.FooterDisplay,
                    Size = spec.Size,
                    ImageSrc = spec.ImageSrc,
                    BgImageSrc = spec.BuildImageSrc(id),
                    IsEnabled = spec.CheckEnable(data, id),
                    IsClickable = spec.CheckEnable(data, id) && spec.ClickId > 0,
                    ClickId = spec.ClickId + id,
                    LcdDisplay = spec.LcdBackground,
                    RenderContent = spec.RenderContent,
                    BodyComponent = spec.BodyComp
                });
            }
        }

        return boxes;
    }

    private static string[] BuildClassificationNames()
    {
        var count = (int)VsBoxKeyRanked.Classification;
        var names = new string[count];

        for (var i = 0; i < count; i++)
            names[i] =
                $"{VsBoxKeyRanked.Classification}{i + 1}";

        return names;
    }

}
public sealed class VsComponentParameters
{
    public Func<Task>? OnTeamSaved { get; init; }
    public EventCallback<bool> OnQuitConfirmationChanged { get; init; }
    public EventCallback<int> OnTeamLevelChanged { get; init; }
    public int ClassificationId { get; init; }
}

/**
 * MÓDOSÍTÁS: a ranked meccs dinamikus komponense már csak a valóban
 * szükséges ClassificationId paramétert, a visszalépési megerősítés
 * állapotát és a tényleges szintlépést közlő EventCallbacket kapja;
 * a kapcsolat- és hibaállapot nem utazik builder callbackeken keresztül.
 *
 * A fájl a VS spec-ekből ContentBox view modelleket épít.
 */

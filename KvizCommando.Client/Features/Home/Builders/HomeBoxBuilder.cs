using KvizCommando.Client.Models.ViewModels;
using KvizCommando.Client.Services.Visual.UiService.Language;
using KvizCommando.Shared.Models.Dtos;

namespace KvizCommando.Client.Features.Home.Builders;

/// <summary>
/// Összeállítja a kezdőképernyő tartalomdobozainak nézetmodelljeit.
/// </summary>
public static class HomeBoxBuilder
{
    /// <summary>A kezdőképernyő dobozainak megjelenítési sorrendje.</summary>
    public static readonly string[] BtnOrder = Enum.GetNames<HomeBoxKey>();

    /// <summary>
    /// Összeállítja a bemeneti adatokhoz tartozó megjelenítési modellt.
    /// </summary>
    /// <param name="hs">A kezdőképernyő forrásadata.</param>
    /// <param name="lang">A feliratok feloldásához használt nyelvi szolgáltatás.</param>
    public static Dictionary<string, ContentBoxVm> Build(HomeScreen hs, ILanguageService lang)
    {
        var dict = new Dictionary<string, ContentBoxVm>(HomeBoxSpecs.Specs.Count);

        foreach (var spec in HomeBoxSpecs.Specs)
        {
            var dictKey = spec.Key.ToString();
            var btn = spec.Pick(hs); // ScreenButtonEntity a DTO-ból

            dict[dictKey] = new ContentBoxVm
            {
                DictKey = dictKey,
                Header = lang[spec.TitleKey],
                Footer = spec.BuildFooter(lang, btn),
                FooterDisplay = spec.FooterDisplay,
                Size = spec.Size,
                ReSizable = spec.ReSizable,
                ImageSrc = spec.ImageSrc,
                BgImageSrc = spec.BgImageSrc,
                IsEnabled = btn.Enable,
                ClickId = spec.ClickId,
                IsClickable = spec.ClickId > 0 && btn.Enable,
                LcdDisplay = spec.LcdBackground && spec.ClickId == 0,
                RenderContent = spec.RenderContent
            };

        }

        return dict;
    }


}

/// <summary>
/// A kezdőképernyő gyökérdobozainak sorrendjét teszi elérhetővé.
/// </summary>
public static class BxOrdHome
{
    /// <summary>A gyökérdobozok neveinek sorrendje.</summary>
    public static readonly string[] Root = Enum.GetNames<HomeBoxKey>();
}

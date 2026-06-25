using KvizCommando.Client.Models.ViewModels;
using KvizCommando.Client.Services.Language;
using KvizCommando.Shared.Models.Dtos;
using System.ComponentModel.DataAnnotations;

namespace KvizCommando.Client.Features.Home;

public static class HomeButtonsBuilder
{
    public static Dictionary<string, ContentBoxVm> Build(HomeScreen hs, ILanguageService lang)
    {
        var dict = new Dictionary<string, ContentBoxVm>(HomeButtonSpecs.Specs.Count);

        foreach (var spec in HomeButtonSpecs.Specs)
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
                ReSizable = string.IsNullOrEmpty(spec.Size),
                ShowImage = !string.IsNullOrEmpty(spec.ImageSrc),
                ImageSrc = spec.ImageSrc,
                BgImageSrc = spec.BgImageSrc,
                IsEnabled = btn.Enable,
                ClickId = spec.ClickId,
                IsClickable = spec.ClickId > 0 && btn.Enable,
                LcdDisplay = spec.LcdBackground && string.IsNullOrEmpty(spec.ImageSrc) && spec.ClickId == 0
            };
           
        }

        return dict;
    }
    

}
public static class BxOrdHome
{
    public static readonly string[] Root = Enum.GetNames<HomeBoxKey>();
}
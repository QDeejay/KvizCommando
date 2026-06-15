using KvizCommando.Client.Models.ViewModels;
using KvizCommando.Client.Services.Language;
using KvizCommando.Shared.Models.Dtos;

namespace KvizCommando.Client.Features.Home;

public static class HomeButtonsBuilder
{
    public static List<ContentBoxVm> Build(HomeScreen hs, ILanguageService lang)
    {

        var list = new List<ContentBoxVm>(HomeButtonSpecs.Specs.Count);

        foreach (var spec in HomeButtonSpecs.Specs)
        {
            var btn = spec.Pick(hs); // ScreenButtonEntity a DTO-ból
            list.Add(new ContentBoxVm
            {
                Header = lang[spec.TitleKey],
                Footer = spec.BuildFooter(lang, btn),
                FooterDisplay = spec.FooterDisplay,
                Size = spec.Size,
                ReSizable = string.IsNullOrEmpty(spec.Size),
                ShowImage = !string.IsNullOrEmpty(spec.ImageSrc),
                ImageSrc = spec.ImageSrc,
                IsEnabled = btn.Enable,
                ClickId = spec.ClickId,
                IsClickable = spec.ClickId > 0,
                LcdDisplay = spec.LcdBackground && string.IsNullOrEmpty(spec.ImageSrc) && spec.ClickId == 0

            });
        }

        return list;
    }
}

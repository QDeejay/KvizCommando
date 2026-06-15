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
                ImageSrc = spec.ImageSrc,
                IsClickable = spec.ClickId>0, 
                IsEnabled = btn.Enable,
                ClickId = spec.ClickId
            });
        }

        return list;
    }
}

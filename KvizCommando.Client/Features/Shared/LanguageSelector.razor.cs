using Blazored.LocalStorage;
using KvizCommando.Client.Features.Shared.Modal.Builders;
using KvizCommando.Client.Features.Shared.Modal.Components;
using KvizCommando.Client.Services.Visual.UiService;
using KvizCommando.Localization;
using Microsoft.AspNetCore.Components;
using System.Globalization;

namespace KvizCommando.Client.Features.Shared
{
    public partial class LanguageSelector
    {
        [Inject] private UiServices Ui { get; set; } = default!;
        [Inject] private ILocalStorageService LocalStorage { get; set; } = default!;

        private Task HuClickAsync() => ShowConfirmAsync("hu-HU");
        private Task EnClickAsync() => ShowConfirmAsync("en-US");

        private async Task ShowConfirmAsync(string cultureName)
        {
            if (!SupportedCultureCatalog.TryGet(cultureName, out var culture) ||
                CultureInfo.CurrentCulture.Name.Equals(
                    culture.Name,
                    StringComparison.OrdinalIgnoreCase))
                return;

            var languageCode = culture.TwoLetterISOLanguageName;

            var modal = MBoxBuilder.BuildParam(
                ModalTypes.LangConfirm,
                Ui.Lang) with
            {
                Title =
                    Ui.Lang[$"common.Modal.Language.Title.{languageCode}"],
                ActionText1 =
                    Ui.Lang[$"common.Modal.Language.Restart.{languageCode}"]
            };

            modal.BodyParameters.Add(
                nameof(DBoxModalRender.DialogBoxType),
                DBoxConfirmTypes.LanguageConfirm);

            modal.BodyParameters.Add(
                nameof(DBoxModalRender.RequestedLanguage),
                languageCode);

            if (await Ui.Modal.ShowAsync(modal) != ModalResult.Button1)
                return;

            await Ui.Lang.ClearLanguageCacheAsync(
                CultureInfo.CurrentCulture.TwoLetterISOLanguageName);

            await LocalStorage.SetItemAsync("userLang", culture.Name);
            Ui.Nav.NavigateTo(Ui.Nav.Uri, forceLoad: true);
        }
    }
}

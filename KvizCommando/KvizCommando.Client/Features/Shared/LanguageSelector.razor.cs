using Blazored.LocalStorage;
using KvizCommando.Client.Features.Shared.Modal.Builders;
using KvizCommando.Client.Features.Shared.Modal.Dynamic;
using KvizCommando.Client.Services.Visual.UiService;
using Microsoft.AspNetCore.Components;
using System.Globalization;

namespace KvizCommando.Client.Features.Shared
{
    public partial class LanguageSelector
    {
        [Inject] private UiServices Ui { get; set; } = default!;
        [Inject] private ILocalStorageService LocalStorage { get; set; } = default!;

        private Task HuClickAsync() => ShowConfirmAsync("hu");
        private Task EnClickAsync() => ShowConfirmAsync("en");

        private async Task ShowConfirmAsync(string languageCode)
        {
            if (CultureInfo.CurrentCulture.TwoLetterISOLanguageName ==
                languageCode)
                return;

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

            var culture = languageCode == "hu"
                ? "hu-HU"
                : "en-US";

            await LocalStorage.SetItemAsync("userLang", culture);
            Ui.Nav.NavigateTo(Ui.Nav.Uri, forceLoad: true);
        }
    }
}

/**
 * MÓDOSÍTÁS: a modal címe és első gombja a kiválasztott új nyelv
 * .hu/.en kulcsából készül. A modal body megkapja a dialogtípust és a
 * kért nyelvkódot; Close vagy X esetén nem történik nyelvváltás.
 */

using Blazored.LocalStorage;
using KvizCommando.Client.Features.Shared.Modal.Dynamic;
using KvizCommando.Client.Features.Shared.Modal.Dynamic.Builders;
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
                Ui.Lang);

            modal.BodyParameters.Add(
                nameof(DBoxModalRender.ConfirmType),
                DBoxConfirmTypes.LanguageConfirm);

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
 * MÓDOSÍTÁS: a nyelvválasztó egyetlen MBoxBuilder hívással készíti el a
 * modalt, átadja a DBoxModalRender ConfirmType paraméterét, awaiteli az
 * eredményt, majd maga menti a kultúrát és tölti újra az oldalt.
 */

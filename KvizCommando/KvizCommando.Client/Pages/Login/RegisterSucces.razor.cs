using KvizCommando.Client.Services.Language;
using Microsoft.AspNetCore.Components;
using System.Globalization;

namespace KvizCommando.Client.Pages.Login
{
    partial class RegisterSucces
    {
        [Inject] private NavigationManager Nav { get; set; } = default!;
        [Inject] private ILanguageService Lang { get; set; } = default!;

        string culture = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
        private void NavigateHome()
        {
            Nav.NavigateTo("/");
        }
    }
}
using Blazored.LocalStorage;
using KvizCommando.Localization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System.Globalization;

namespace KvizCommando.Client.DependencyInjection;

public static class ClientLocalizationExtensions
{
    public static IServiceCollection AddClientLocalization(
        this IServiceCollection services)
    {
        services.AddLocalization();

        return services;
    }

    public static async Task InitializeClientLocalizationAsync(
        this WebAssemblyHost host)
    {
        var navigation = host.Services.GetRequiredService<NavigationManager>();
        var localStorage = host.Services.GetRequiredService<ILocalStorageService>();

        var uri = navigation.ToAbsoluteUri(navigation.Uri);
        var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
        var queryCulture = query["culture"];
        var storedCulture = await localStorage.GetItemAsync<string>("userLang");

        var culture = SupportedCultureCatalog.Resolve(
            queryCulture,
            storedCulture);

        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;
        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;

        await localStorage.SetItemAsync("userLang", culture.Name);
    }
}

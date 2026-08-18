using KvizCommando.Client.Helpers;
using KvizCommando.Client.Services.Audio;
using KvizCommando.Client.Services.Visual;
using KvizCommando.Client.Services.Visual.UiService;
using KvizCommando.Client.Services.Visual.UiService.Language;

namespace KvizCommando.Client.DependencyInjection;

public static class ClientUiServiceExtensions
{
    /// <summary>
    /// Regisztrálja a lokalizáció, a felületi állapotjelzések, a hang és a nézetépítés szolgáltatásait.
    /// </summary>
    /// <param name="services">A bővítendő szolgáltatásgyűjtemény.</param>
    /// <returns>A további regisztrációkhoz használható szolgáltatásgyűjtemény.</returns>
    public static IServiceCollection AddClientUiServices(
        this IServiceCollection services)
    {
        services.AddScoped<ILanguageService, LanguageService>();
        services.AddSingleton<IDisplayMessageState, DisplayMessageState>();
        services.AddScoped<PageHeaderService>();
        services.AddScoped<ModalService>();
        services.AddSingleton<ToastService>();
        services.AddSingleton<SubHeaderService>();
        services.AddScoped<UiServices>();
        services.AddScoped<MarkupLoaderService>();
        services.AddScoped<CategoryOptionHelpers>();
        services.AddSingleton<AudioService>();
        services.AddSingleton<LoaderService>();
        services.AddSingleton<ICategoryLookupService, StaticCategoryLookupService>();

        return services;
    }
}

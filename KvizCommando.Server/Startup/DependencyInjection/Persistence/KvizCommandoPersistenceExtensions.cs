using KvizCommando.Server.Infrastructure.Persistence;
using KvizCommando.Server.Services.Db;
using Microsoft.EntityFrameworkCore;

namespace KvizCommando.Server.Startup;

public static class KvizCommandoPersistenceExtensions
{
    /// <summary>
    /// Regisztrálja az Identity- és játékadatbázist, valamint az ezeket közvetlenül elérő szolgáltatásokat.
    /// </summary>
    /// <param name="services">A bővítendő szolgáltatásgyűjtemény.</param>
    /// <param name="configuration">Az adatbázis-kapcsolati karakterláncokat tartalmazó konfiguráció.</param>
    /// <returns>A további regisztrációkhoz használható szolgáltatásgyűjtemény.</returns>
    public static IServiceCollection AddKvizCommandoPersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseSqlite(
                configuration.GetConnectionString("DefaultConnection"));

            // SQL Server alternatíva; a központi szolgáltatókapcsoló elkészültéig kikommentelve marad.
            // options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"),
            //     sqlOptions => sqlOptions.EnableRetryOnFailure());
        });

        services.AddDbContext<GameDbContext>(options =>
        {
            options.UseSqlite(
                configuration.GetConnectionString("GameConnection"));

            // SQL Server alternatíva; a központi szolgáltatókapcsoló elkészültéig kikommentelve marad.
            // options.UseSqlServer(configuration.GetConnectionString("GameConnection"),
            //     sqlOptions => sqlOptions.EnableRetryOnFailure());
        });

        services.AddScoped<IQuestionDbService, QuestionDbService>();
        services.AddScoped<IPlayerDbService, PlayerDbService>();

        return services;
    }
}

using KvizCommando.Server.Domain.Entities.Questions;
using KvizCommando.Server.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;

namespace KvizCommando.Server.Infrastructure.Persistence
{
    /// <summary>A gyári és felhasználói kérdések adatbáziskontextusa.</summary>
    public class GameDbContext : DbContext
    {
        /// <summary>Létrehozza a játék futás közben használt kérdés-adatbáziskontextusát.</summary>
        public GameDbContext(DbContextOptions<GameDbContext> options)
            : base(options)
        {
        }

        /// <summary>
        /// A providerenként külön migrációs kontextusok közös konstruktora.
        /// </summary>
        protected GameDbContext(DbContextOptions options)
            : base(options)
        {
        }

        public DbSet<GuessQuestion> GuessQuestions { get; set; }
        public DbSet<FactoryQuestion> FactoryQuestions { get; set; }
        public DbSet<UserQuestion> UserQuestions { get; set; }
        public DbSet<PendingQuestion> PendingQuestions { get; set; }
        /// <inheritdoc />
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfiguration(new UserQuestionConfiguration());
            modelBuilder.ApplyConfiguration(new PendingQuestionConfiguration());

            if (Database.IsSqlite())
                SqliteModelConfiguration.ConfigureGame(modelBuilder);
            else if (Database.IsSqlServer())
                SqlServerModelConfiguration.ConfigureGame(modelBuilder);
            else
                throw new InvalidOperationException(
                    $"Nem támogatott adatbázis-provider: {Database.ProviderName}");
        }

    }
}

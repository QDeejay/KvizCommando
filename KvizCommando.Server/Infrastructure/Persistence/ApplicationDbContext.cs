using KvizCommando.Server.Domain.Entities.Compliance;
using KvizCommando.Server.Domain.Entities.Players;
using KvizCommando.Server.Domain.Entities.Statistics;
using KvizCommando.Server.Domain.Entities.Billing;
using KvizCommando.Server.Domain.Entities.Security;
using KvizCommando.Server.Identity;
using KvizCommando.Server.Infrastructure.Persistence.Configurations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace KvizCommando.Server.Infrastructure.Persistence
{
    /// <summary>
    /// Az Identity-, játékos- és fiókhoz kapcsolódó adatok közös adatbáziskontextusa.
    /// </summary>
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole, string,
        IdentityUserClaim<string>, IdentityUserRole<string>, IdentityUserLogin<string>,
        IdentityRoleClaim<string>, ApplicationUserToken>
    {
        /// <summary>Létrehozza az alkalmazás futás közben használt adatbáziskontextusát.</summary>
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        /// <summary>
        /// A providerenként külön migrációs kontextusok közös konstruktora.
        /// </summary>
        protected ApplicationDbContext(DbContextOptions options)
            : base(options) { }

        public DbSet<Player> Players => Set<Player>();
        public DbSet<PlayerCharacter> PlayerCharacters => Set<PlayerCharacter>();
        public DbSet<PlayerLoadout> PlayerLoadouts => Set<PlayerLoadout>();
        public DbSet<PlayerCategoryStat> PlayerCategoryStats => Set<PlayerCategoryStat>();

        public DbSet<PlayerOrientStat> PlayerOrientStat => Set<PlayerOrientStat>();
        public DbSet<PlayerAskStats> PlayerAskStats => Set<PlayerAskStats>();
        public DbSet<TeamStatistic> TeamStatistics => Set<TeamStatistic>();

        public DbSet<TermsConsent> TermsConsents => Set<TermsConsent>();
        public DbSet<MarketingConsent> MarketingConsents => Set<MarketingConsent>();
        public DbSet<RegistrationBenefitClaim> RegistrationBenefitClaims => Set<RegistrationBenefitClaim>();

        public DbSet<UserPii> UserPii => Set<UserPii>();
        public DbSet<UserPaymentMethod> UserPaymentMethods => Set<UserPaymentMethod>();

        /// <inheritdoc />
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // A két DbContext határa szándékosan explicit. Így a Questions
            // konfigurációk nem kerülhetnek véletlenül az Application adatbázisba.
            modelBuilder.ApplyConfiguration(new ApplicationUserConfiguration());
            modelBuilder.ApplyConfiguration(new ApplicationUserTokenConfiguration());
            modelBuilder.ApplyConfiguration(new MarketingConsentConfiguration());
            modelBuilder.ApplyConfiguration(new RegistrationBenefitClaimConfiguration());
            modelBuilder.ApplyConfiguration(new TermsConsentConfiguration());
            modelBuilder.ApplyConfiguration(new UserPaymentMethodConfiguration());
            modelBuilder.ApplyConfiguration(new UserPiiConfiguration());
            modelBuilder.ApplyConfiguration(new PlayerAskStatsConfiguration());
            modelBuilder.ApplyConfiguration(new PlayerCategoryStatConfiguration());
            modelBuilder.ApplyConfiguration(new PlayerCharactersConfiguration());
            modelBuilder.ApplyConfiguration(new PlayerConfiguration());
            modelBuilder.ApplyConfiguration(new PlayerLoadoutConfiguration());
            modelBuilder.ApplyConfiguration(new PlayerOrientStatConfiguration());
            modelBuilder.ApplyConfiguration(new TeamStatisticConfiguration());

            if (Database.IsSqlite())
                SqliteModelConfiguration.ConfigureApplication(modelBuilder);
            else if (Database.IsSqlServer())
                SqlServerModelConfiguration.ConfigureApplication(modelBuilder);
            else
                throw new InvalidOperationException(
                    $"Nem támogatott adatbázis-provider: {Database.ProviderName}");
        }
    }
}

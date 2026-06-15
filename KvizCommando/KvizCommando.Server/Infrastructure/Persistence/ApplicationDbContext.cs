using KvizCommando.Server.Domain.Entities.Compliance;
using KvizCommando.Server.Domain.Entities.Players;
using KvizCommando.Server.Domain.Entities.Statistics;
using KvizCommando.Server.Domain.Entities.Billing;
using KvizCommando.Server.Domain.Entities.Security;
using KvizCommando.Server.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace KvizCommando.Server.Infrastructure.Persistence
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole, string,
        IdentityUserClaim<string>, IdentityUserRole<string>, IdentityUserLogin<string>,
        IdentityRoleClaim<string>, ApplicationUserToken>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Player> Players => Set<Player>();
        public DbSet<PlayerCharacter> PlayerCharacters => Set<PlayerCharacter>();
        public DbSet<PlayerLoadout> PlayerLoadouts => Set<PlayerLoadout>();
        public DbSet<PlayerCategoryStat> PlayerCategoryStats => Set<PlayerCategoryStat>();

        public DbSet<PlayerOrientStat> PlayerOrientStat => Set<PlayerOrientStat>();
        public DbSet<PlayerAskStats> PlayerAskStats => Set<PlayerAskStats>();

        public DbSet<TermsConsent> TermsConsents => Set<TermsConsent>();
        public DbSet<MarketingConsent> MarketingConsents => Set<MarketingConsent>();

        public DbSet<UserPii> UserPii => Set<UserPii>();
        public DbSet<UserPaymentMethod> UserPaymentMethods => Set<UserPaymentMethod>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Konfigurációk külön osztályokban
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        }
    }
}
//  Tools manage nutget packages ---> Package manager Console --->
//  Add-Migration ---Ide adunk egy bármilyen nevet pl: AddRankTable vagy CreateInitTable   --- > 
//  Update-Database

/// Add-Migration InitialIdentity -Context ApplicationDbContext -OutputDir "Data\Migrations\Identity"
/// Update-Database -Context ApplicationDbContext
/// 
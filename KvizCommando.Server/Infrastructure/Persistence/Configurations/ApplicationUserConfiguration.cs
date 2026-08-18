using KvizCommando.Server.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KvizCommando.Server.Infrastructure.Persistence.Configurations
{
    public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
    {
        public void Configure(EntityTypeBuilder<ApplicationUser> b)
        {
            // Oszlophosszak a kijelzőnévhez
            b.Property(u => u.DisplayName).HasMaxLength(256);
            b.Property(u => u.NormalizedDisplayName).HasMaxLength(256);

            // Szűrt egyedi index élő userekre (IsDeleted = 0), csak ha nem null a név
            b.HasIndex(u => u.NormalizedDisplayName)
             .HasDatabaseName("UX_AspNetUsers_NormalizedDisplayName_Active")
             .IsUnique()
             .HasFilter("[NormalizedDisplayName] IS NOT NULL AND [IsDeleted] = 0"); // SQL Server filter, SQLite figyelmen kívül hagyja

            // Számított oszlop a check-in kész állapotra
            /// sqlite syntax
            //b.Property(u => u.IsCheckInCompleted)
             //.HasColumnType("INTEGER")
             //.HasComputedColumnSql(
             //    "CASE WHEN [DisplayName] IS NOT NULL AND [AcceptTerms] = 1 THEN 1 ELSE 0 END",
             //    stored: false);

            /// sql server syntax
            //
            //b.Property(u => u.IsCheckInCompleted)
            // .HasComputedColumnSql(
            //     "CASE WHEN [DisplayName] IS NOT NULL AND [AcceptTerms] = 1 THEN CAST(1 AS bit) ELSE CAST(0 AS bit) END",
            //     stored: true);
            //

            // Alapértékek
            b.Property(u => u.PreferredLocale).HasMaxLength(16).HasDefaultValue("hu-HU");

            // Token bővítmény táblanév konfiguráció az Identity default sémához igazodva
            b.Metadata.Model.FindEntityType(typeof(ApplicationUserToken))!
                .SetTableName("AspNetUserTokens");
        }
    }
}

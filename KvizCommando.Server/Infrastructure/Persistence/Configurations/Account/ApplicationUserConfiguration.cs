using KvizCommando.Server.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KvizCommando.Server.Infrastructure.Persistence.Configurations
{
    public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
    {
        /// <inheritdoc />
        public void Configure(EntityTypeBuilder<ApplicationUser> b)
        {
            b.Property(u => u.DisplayName).HasMaxLength(256);
            b.Property(u => u.NormalizedDisplayName).HasMaxLength(256);

            // A törölt felhasználók neve újra felhasználható, az aktív nevek viszont egyediek.
            b.HasIndex(u => u.NormalizedDisplayName)
             .HasDatabaseName("UX_AspNetUsers_NormalizedDisplayName_Active")
             .IsUnique()
             .HasFilter("[NormalizedDisplayName] IS NOT NULL AND [IsDeleted] = 0");

            b.Property(u => u.PreferredLocale).HasMaxLength(16).HasDefaultValue("hu-HU");

            // A bővített tokenentitás az Identity alapértelmezett tábláját használja.
            b.Metadata.Model.FindEntityType(typeof(ApplicationUserToken))!
                .SetTableName("AspNetUserTokens");
        }
    }
}

using KvizCommando.Server.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KvizCommando.Server.Infrastructure.Persistence.Configurations
{
    public class ApplicationUserTokenConfiguration : IEntityTypeConfiguration<ApplicationUserToken>
    {
        public void Configure(EntityTypeBuilder<ApplicationUserToken> b)
        {
            // IdentityUserToken PK: UserId + LoginProvider + Name
            b.HasKey(t => new { t.UserId, t.LoginProvider, t.Name });

            b.Property(t => t.ExpiresAt).IsRequired();

            b.HasIndex(t => t.ExpiresAt)
             .HasDatabaseName("IX_AspNetUserTokens_ExpiresAt");
        }
    }
}

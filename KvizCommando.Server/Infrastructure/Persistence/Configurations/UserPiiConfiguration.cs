using KvizCommando.Server.Domain.Entities.Security;
using KvizCommando.Server.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KvizCommando.Server.Infrastructure.Persistence.Configurations
{
    /// <summary>
    /// Meghatározza a titkosított felhasználói PII-rekord adatbázis-leképezését.
    /// </summary>
    public class UserPiiConfiguration : IEntityTypeConfiguration<UserPii>
    {
        /// <inheritdoc />
        public void Configure(EntityTypeBuilder<UserPii> b)
        {
            b.ToTable("UserPii");

            b.HasKey(x => x.UserId);

            b.HasOne<ApplicationUser>()
                .WithOne()
                .HasForeignKey<UserPii>(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            b.Property(x => x.CreatedUtc).IsRequired();
            b.Property(x => x.UpdatedUtc).IsRequired();

            // A konkrét adattípusokat a kiválasztott adatbázis-szolgáltató határozza meg,
            // ezért ez a leképezés SQLite és SQL Server alatt is használható.
        }
    }
}

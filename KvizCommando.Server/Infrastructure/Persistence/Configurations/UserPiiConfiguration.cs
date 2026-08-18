using KvizCommando.Server.Domain.Entities.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KvizCommando.Server.Infrastructure.Persistence.Configurations
{
    public class UserPiiConfiguration : IEntityTypeConfiguration<UserPii>
    {
        /// <summary>
        /// Beállítja az entitás EF Core leképezését és adatbázis-korlátait.
        /// </summary>
        public void Configure(EntityTypeBuilder<UserPii> b)
        {
            b.ToTable("UserPii");

            b.HasKey(x => x.UserId);

            // Az e-mail-hash egyedisége megakadályozza ugyanazon cím többszöri regisztrálását.
            b.HasIndex(x => x.EmailNormHash)
             .IsUnique()
             .HasDatabaseName("UX_UserPii_EmailNormHash")
             .HasFilter("[EmailNormHash] IS NOT NULL");

            // A telefonszám-hash kereshető, de üzleti szabály nem követeli meg az egyediségét.
            b.HasIndex(x => x.PhoneNormHash)
             .HasDatabaseName("IX_UserPii_PhoneNormHash");

            b.Property(x => x.CreatedUtc).IsRequired();
            b.Property(x => x.UpdatedUtc).IsRequired();

            // A konkrét adattípusokat a kiválasztott adatbázis-szolgáltató határozza meg,
            // ezért ez a leképezés SQLite és SQL Server alatt is használható.
        }
    }
}

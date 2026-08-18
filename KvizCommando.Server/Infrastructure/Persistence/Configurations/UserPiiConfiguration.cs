using KvizCommando.Server.Domain.Entities.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KvizCommando.Server.Infrastructure.Persistence.Configurations
{
    public class UserPiiConfiguration : IEntityTypeConfiguration<UserPii>
    {
        public void Configure(EntityTypeBuilder<UserPii> b)
        {
            b.ToTable("UserPii");

            b.HasKey(x => x.UserId);

            // Email hash egyedi (ha akarod biztosítani az egyediséget)
            b.HasIndex(x => x.EmailNormHash)
             .IsUnique()
             .HasDatabaseName("UX_UserPii_EmailNormHash")
             .HasFilter("[EmailNormHash] IS NOT NULL");

            // Phone hash index (nem feltétlen unique)
            b.HasIndex(x => x.PhoneNormHash)
             .HasDatabaseName("IX_UserPii_PhoneNormHash");

            // Kötelező dátumok
            b.Property(x => x.CreatedUtc).IsRequired();
            b.Property(x => x.UpdatedUtc).IsRequired();

            // Típus-affinitások SQLite/SQL Serverhez: nem erőltetünk konkrét típusokat,
            // a byte[] → varbinary/blob, string → nvarchar/text minden provideren működik.
        }
    }
}

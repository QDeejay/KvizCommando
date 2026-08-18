using KvizCommando.Server.Domain.Entities.Compliance;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KvizCommando.Server.Infrastructure.Persistence.Configurations
{
    public class TermsConsentConfiguration : IEntityTypeConfiguration<TermsConsent>
    {
        /// <summary>
        /// Beállítja az entitás EF Core leképezését és adatbázis-korlátait.
        /// </summary>
        public void Configure(EntityTypeBuilder<TermsConsent> b)
        {
            b.ToTable("TermsConsents");

            b.HasKey(x => x.Id);
            b.Property(x => x.Id).ValueGeneratedOnAdd();

            b.Property(x => x.UserId).IsRequired();
            b.Property(x => x.TermsVersion).HasMaxLength(32).IsRequired();
            b.Property(x => x.AcceptedAtUtc).IsRequired();

            // A HMAC-SHA256 eredménye 32 bájt. SQLite BLOB mezőn nem kényszeríti ki a maximális hosszt.
            b.Property(x => x.UserAgentHash).HasMaxLength(32);
            b.Property(x => x.IpHash).HasMaxLength(32);

            // Egy felhasználó egy ÁSZF-verziót legfeljebb egyszer fogadhat el.
            b.HasIndex(x => new { x.UserId, x.TermsVersion })
             .IsUnique()
             .HasDatabaseName("UX_TermsConsents_UserId_TermsVersion");

            // Az index az utolsó elfogadás lekérdezési sorrendjét követi.
            b.HasIndex(x => new { x.UserId, x.AcceptedAtUtc })
            .IsDescending(false, true)
            .HasDatabaseName("IX_TermsConsents_UserId_AcceptedAtUtc");



            // Szolgáltatófüggő CHECK constraint minták. A központi adatbázis-kapcsoló
            // bevezetésekor pontosan az aktív szolgáltatóhoz tartozó blokk engedélyezhető.
            // SQLite BLOB mezőn a length() a bájtok számát adja vissza.
            // b.ToTable(t => {
            //     t.HasCheckConstraint("CK_TermsConsents_UserAgentHash_Len",
            //         "UserAgentHash IS NULL OR length(UserAgentHash) = 32");
            //     t.HasCheckConstraint("CK_TermsConsents_IpHash_Len",
            //         "IpHash IS NULL OR length(IpHash) = 32");
            // });

            // SQL Server varbinary mezőn a DATALENGTH adja vissza a bájtok számát.
            // b.ToTable(t => {
            //     t.HasCheckConstraint("CK_TermsConsents_UserAgentHash_Len",
            //         "UserAgentHash IS NULL OR DATALENGTH(UserAgentHash) = 32");
            //     t.HasCheckConstraint("CK_TermsConsents_IpHash_Len",
            //         "IpHash IS NULL OR DATALENGTH(IpHash) = 32");
            // });
        }
    }
}

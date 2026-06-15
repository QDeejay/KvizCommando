using KvizCommando.Server.Domain.Entities.Compliance;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KvizCommando.Server.Infrastructure.Persistence.Configurations
{
    public class TermsConsentConfiguration : IEntityTypeConfiguration<TermsConsent>
    {
        public void Configure(EntityTypeBuilder<TermsConsent> b)
        {
            b.ToTable("TermsConsents");

            b.HasKey(x => x.Id);
            b.Property(x => x.Id).ValueGeneratedOnAdd();

            b.Property(x => x.UserId).IsRequired();
            b.Property(x => x.TermsVersion).HasMaxLength(32).IsRequired();
            b.Property(x => x.AcceptedAtUtc).IsRequired();

            // HMAC-SHA256 → 32 byte (256 bit). SQLite nem érvényesíti a max length-et BLOB-on,
            // de a HMAC miatt alkalmazásoldalon így is fix 32 bájt lesz.
            b.Property(x => x.UserAgentHash).HasMaxLength(32);
            b.Property(x => x.IpHash).HasMaxLength(32);

            // 1) Append-only audit: minden elfogadott verzió külön sor
            b.HasIndex(x => new { x.UserId, x.TermsVersion })
             .IsUnique()
             .HasDatabaseName("UX_TermsConsents_UserId_TermsVersion");

            // 2) Gyors "utolsó elfogadás" lekérdezéshez
            b.HasIndex(x => new { x.UserId, x.AcceptedAtUtc })
            .IsDescending(false, true)   // UserId ASC, AcceptedAtUtc DESC
            .HasDatabaseName("IX_TermsConsents_UserId_AcceptedAtUtc");



            // --- Opcionális, SZIGORÚ hossz-ellenőrzés a hash oszlopokra CHECK constrainttel ---

            // SQLITE (főcsapás): a length() függvény bájtok számát adja meg BLOB-on is.
            // Engedélyezéshez vedd ki a kommentet.
            // b.ToTable(t => {
            //     t.HasCheckConstraint("CK_TermsConsents_UserAgentHash_Len",
            //         "UserAgentHash IS NULL OR length(UserAgentHash) = 32");
            //     t.HasCheckConstraint("CK_TermsConsents_IpHash_Len",
            //         "IpHash IS NULL OR length(IpHash) = 32");
            // });

            // SQL SERVER (alternatíva): DATALENGTH(varbinary) adja meg a bájtok számát.
            // Ha SQL Servert használsz, ezt kapcsold be az előző SQLITE blokk helyett.
            // b.ToTable(t => {
            //     t.HasCheckConstraint("CK_TermsConsents_UserAgentHash_Len",
            //         "UserAgentHash IS NULL OR DATALENGTH(UserAgentHash) = 32");
            //     t.HasCheckConstraint("CK_TermsConsents_IpHash_Len",
            //         "IpHash IS NULL OR DATALENGTH(IpHash) = 32");
            // });
        }
    }
}

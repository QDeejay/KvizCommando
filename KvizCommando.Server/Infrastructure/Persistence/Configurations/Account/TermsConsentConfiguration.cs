using KvizCommando.Server.Domain.Entities.Compliance;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KvizCommando.Server.Infrastructure.Persistence.Configurations
{
    public class TermsConsentConfiguration : IEntityTypeConfiguration<TermsConsent>
    {
        /// <inheritdoc />
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



            // A providerfüggő hosszellenőrzést a központi modellkonfiguráció adja hozzá.
        }
    }
}

using KvizCommando.Server.Domain.Entities.Compliance;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KvizCommando.Server.Infrastructure.Persistence.Configurations
{
    public class MarketingConsentConfiguration : IEntityTypeConfiguration<MarketingConsent>
    {
        public void Configure(EntityTypeBuilder<MarketingConsent> b)
        {
            b.ToTable("MarketingConsents");

            b.HasKey(x => x.Id);
            b.Property(x => x.Id).ValueGeneratedOnAdd();

            b.Property(x => x.UserId).IsRequired();
            b.Property(x => x.Granted).IsRequired();
            b.Property(x => x.ChangedAtUtc).IsRequired();

            b.HasIndex(x => new { x.UserId, x.ChangedAtUtc })
             .HasDatabaseName("IX_MarketingConsents_UserId_ChangedAtUtc");
        }
    }
}

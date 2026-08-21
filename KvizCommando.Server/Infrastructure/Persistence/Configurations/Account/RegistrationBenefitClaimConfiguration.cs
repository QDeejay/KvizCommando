using KvizCommando.Server.Domain.Entities.Compliance;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KvizCommando.Server.Infrastructure.Persistence.Configurations;

public sealed class RegistrationBenefitClaimConfiguration : IEntityTypeConfiguration<RegistrationBenefitClaim>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<RegistrationBenefitClaim> builder)
    {
        builder.ToTable("RegistrationBenefitClaims");
        builder.HasKey(x => x.EmailFingerprint);
        builder.Property(x => x.EmailFingerprint).HasMaxLength(64);
        builder.Property(x => x.EligibleAgainAtUtc).IsRequired();
    }
}

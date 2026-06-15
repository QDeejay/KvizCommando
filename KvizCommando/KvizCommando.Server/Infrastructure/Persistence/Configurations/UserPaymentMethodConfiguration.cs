using KvizCommando.Server.Domain.Entities.Billing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KvizCommando.Server.Infrastructure.Persistence.Configurations
{
    public class UserPaymentMethodConfiguration : IEntityTypeConfiguration<UserPaymentMethod>
    {
        public void Configure(EntityTypeBuilder<UserPaymentMethod> b)
        {
            b.ToTable("UserPaymentMethods");

            b.HasKey(x => x.Id);

            b.Property(x => x.UserId).IsRequired();
            b.Property(x => x.Processor).HasMaxLength(32).IsRequired();
            b.Property(x => x.PaymentMethodToken).HasMaxLength(256).IsRequired();

            b.Property(x => x.CardBrand).HasMaxLength(32);
            b.Property(x => x.CardLast4).HasMaxLength(4);

            b.HasIndex(x => new { x.UserId, x.IsDefault })
             .HasDatabaseName("IX_UserPaymentMethods_User_IsDefault");

            b.Property(x => x.CreatedUtc).IsRequired();
            b.Property(x => x.UpdatedUtc).IsRequired();
        }
    }
}

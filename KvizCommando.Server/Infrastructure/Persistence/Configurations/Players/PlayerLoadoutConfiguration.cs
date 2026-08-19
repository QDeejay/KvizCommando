using KvizCommando.Server.Domain.Entities.Players;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KvizCommando.Server.Infrastructure.Persistence.Configurations
{
    public class PlayerLoadoutConfiguration : IEntityTypeConfiguration<PlayerLoadout>
    {
        /// <inheritdoc />
        public void Configure(EntityTypeBuilder<PlayerLoadout> b)
        {
            b.ToTable("PlayerLoadouts");

            b.HasKey(pl => pl.PlayerId);
            b.Property(pl => pl.PlayerId)
             .ValueGeneratedNever();

            b.Property(pl => pl.FactorySlotsJson).IsRequired();
            b.Property(pl => pl.UserSlotsJson).IsRequired();
            b.Property(pl => pl.PendingSlotsJson).IsRequired();

            // Az oszloptípust és a JSON-ellenőrzést a provider konfigurációja adja meg.

            b.Property(pl => pl.UpdatedUtc).IsRequired();
        }
    }
}

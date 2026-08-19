using KvizCommando.Server.Domain.Entities.Players;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KvizCommando.Server.Infrastructure.Persistence.Configurations
{
    public class PlayerCharactersConfiguration : IEntityTypeConfiguration<PlayerCharacter>
    {
        /// <inheritdoc />
        public void Configure(EntityTypeBuilder<PlayerCharacter> b)
        {
            b.ToTable("PlayerCharacters");

            b.HasKey(pc => pc.PlayerId);
            b.Property(pc => pc.PlayerId)
             .ValueGeneratedNever();

            b.Property(pc => pc.CharactersJson)
             .IsRequired();

            b.Property(pc => pc.CandidatesJson)
            .IsRequired();

            // Az oszloptípust és a JSON-ellenőrzést a provider konfigurációja adja meg.

            b.Property(pc => pc.UpdatedUtc).IsRequired();
        }
    }
}

using KvizCommando.Server.Domain.Entities.Players;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KvizCommando.Server.Infrastructure.Persistence.Configurations
{
    public class PlayerConfiguration : IEntityTypeConfiguration<Player>
    {
        /// <inheritdoc />
        public void Configure(EntityTypeBuilder<Player> b)
        {
            b.ToTable("Players");

            b.HasKey(p => p.PlayerId);

            b.Property(p => p.PlayerId).ValueGeneratedOnAdd();
            b.Property(p => p.UserId).IsRequired();

            b.HasIndex(p => p.UserId)
             .IsUnique()
             .HasDatabaseName("UX_Players_UserId");

            b.Property(p => p.TeamName)
             .IsRequired()
             .HasMaxLength(20);

            b.Property(p => p.NormalizedTeamName)
             .IsRequired()
             .HasMaxLength(20);

            b.HasIndex(p => p.NormalizedTeamName)
             .IsUnique()
             .HasDatabaseName("UX_Players_NormalizedTeamName");

            b.Property(p => p.CaptainAvatar)
             .IsRequired()
             .HasMaxLength(64);

            b.Property(p => p.TeamNameChangedUtc)
             .IsRequired(false);

            // A rangsor lekérdezéseit külön XP- és kreditindex támogatja.
            b.HasIndex(p => p.XP)
             .HasDatabaseName("IX_Players_XP");

            b.HasIndex(p => p.Credit)
             .HasDatabaseName("IX_Players_Credit");

            b.Property(p => p.CreatedUtc).IsRequired();
            b.Property(p => p.UpdatedUtc).IsRequired();

            // A RowVersion működését a provider konfigurációja adja meg.
        }
    }
}

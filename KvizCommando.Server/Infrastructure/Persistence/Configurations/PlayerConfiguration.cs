using KvizCommando.Server.Domain.Entities.Players;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KvizCommando.Server.Infrastructure.Persistence.Configurations
{
    public class PlayerConfiguration : IEntityTypeConfiguration<Player>
    {
        public void Configure(EntityTypeBuilder<Player> b)
        {
            b.ToTable("Players");

            b.HasKey(p => p.PlayerId);

            b.Property(p => p.PlayerId).ValueGeneratedOnAdd();
            b.Property(p => p.UserId).IsRequired();

            b.HasIndex(p => p.UserId)
             .IsUnique()
             .HasDatabaseName("UX_Players_UserId");

            b.Property(p => p.TeamName).HasMaxLength(128);

            // Leaderboard indexek
            b.HasIndex(p => p.XP)
             .HasDatabaseName("IX_Players_XP");

            b.HasIndex(p => p.Credit)
             .HasDatabaseName("IX_Players_Credit");

            // Opcionális kombinált
            // b.HasIndex(p => new { p.XP, p.Credit }).HasDatabaseName("IX_Players_XP_Credit");

            b.Property(p => p.CreatedUtc).IsRequired();
            b.Property(p => p.UpdatedUtc).IsRequired();

            /// sqlite verziótól függően:
            b.Property(p => p.RowVersion)
            // .IsRowVersion()    // SQLite nem támogatja szervernél visszakapcsolni
             .IsConcurrencyToken();
        }
    }
}

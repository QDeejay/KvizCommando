using KvizCommando.Server.Domain.Entities.Statistics;
using KvizCommando.Server.Domain.Entities.Players;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KvizCommando.Server.Infrastructure.Persistence.Configurations
{
    public class PlayerOrientStatConfiguration : IEntityTypeConfiguration<PlayerOrientStat>
    {
        /// <inheritdoc />
        public void Configure(EntityTypeBuilder<PlayerOrientStat> b)
        {
            b.ToTable("PlayerOrientStat");

            b.HasKey(x => new { x.PlayerId, x.OrientId });

            b.HasOne<Player>()
             .WithMany()
             .HasForeignKey(x => x.PlayerId)
             .OnDelete(DeleteBehavior.Cascade);

            b.Property(x => x.OrientId).HasColumnType("smallint");

            
            b.Property(x => x.HighScore).IsRequired();
            b.Property(x => x.HighScoreTime).IsRequired();

            

            
            b.HasIndex(x => x.OrientId)
             .HasDatabaseName("IX_PlayerOrientStat_CategoryId");

            
        }
    }
}

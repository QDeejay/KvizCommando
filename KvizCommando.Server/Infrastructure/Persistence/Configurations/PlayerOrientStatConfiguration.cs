using KvizCommando.Server.Domain.Entities.Statistics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KvizCommando.Server.Infrastructure.Persistence.Configurations
{
    public class PlayerOrientStatConfiguration : IEntityTypeConfiguration<PlayerOrientStat>
    {
        /// <summary>
        /// Beállítja az entitás EF Core leképezését és adatbázis-korlátait.
        /// </summary>
        public void Configure(EntityTypeBuilder<PlayerOrientStat> b)
        {
            b.ToTable("PlayerOrientStat");

            b.HasKey(x => new { x.PlayerId, x.OrientId });

            b.Property(x => x.OrientId).HasColumnType("smallint");

            
            b.Property(x => x.HighScore).IsRequired();
            b.Property(x => x.HighScoreTime).IsRequired();

            

            
            b.HasIndex(x => x.OrientId)
             .HasDatabaseName("IX_PlayerOrientStat_CategoryId");

            
        }
    }
}

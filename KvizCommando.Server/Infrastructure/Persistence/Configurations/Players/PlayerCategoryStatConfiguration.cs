using KvizCommando.Server.Domain.Entities.Statistics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KvizCommando.Server.Infrastructure.Persistence.Configurations
{
    public class PlayerCategoryStatConfiguration : IEntityTypeConfiguration<PlayerCategoryStat>
    {
        /// <inheritdoc />
        public void Configure(EntityTypeBuilder<PlayerCategoryStat> b)
        {
            b.ToTable("PlayerCategoryStats");

            b.HasKey(x => new { x.PlayerId, x.CategoryId });

            b.Property(x => x.CategoryId).HasColumnType("smallint");

            b.Property(x => x.Answered).IsRequired();
            b.Property(x => x.Correct).IsRequired();
            b.Property(x => x.HighScore).IsRequired();
            b.Property(x => x.HighScoreTime).IsRequired();

            // A számított oszlop SQL-kifejezését a provider konfigurációja adja meg.

            // Kategóriánkénti statisztikai lekérdezések
            b.HasIndex(x => x.CategoryId)
             .HasDatabaseName("IX_PlayerCategoryStats_CategoryId");

            // Helyességi arány szerinti rangsor
            b.HasIndex(x => x.Ratio)
             .HasDatabaseName("IX_PlayerCategoryStats_Ratio_DESC");
        }
    }
}

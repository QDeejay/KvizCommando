using KvizCommando.Server.Domain.Entities.Statistics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KvizCommando.Server.Infrastructure.Persistence.Configurations
{
    public class PlayerCategoryStatConfiguration : IEntityTypeConfiguration<PlayerCategoryStat>
    {
        /// <summary>
        /// Beállítja az entitás EF Core leképezését és adatbázis-korlátait.
        /// </summary>
        public void Configure(EntityTypeBuilder<PlayerCategoryStat> b)
        {
            b.ToTable("PlayerCategoryStats");

            b.HasKey(x => new { x.PlayerId, x.CategoryId });

            b.Property(x => x.CategoryId).HasColumnType("smallint");

            b.Property(x => x.Answered).IsRequired();
            b.Property(x => x.Correct).IsRequired();
            b.Property(x => x.HighScore).IsRequired();
            b.Property(x => x.HighScoreTime).IsRequired();

            // SQLite számított oszlop a helyességi arányhoz.
            b.Property(x => x.Ratio)
             .HasColumnType("REAL")
             .HasComputedColumnSql(
                 "CASE WHEN [Answered] = 0 THEN 0.0 ELSE (1.0 * [Correct] / [Answered]) END",
                 stored: false);

            // SQL Server alternatíva
            // b.Property(x => x.Ratio)
            //  .HasColumnType("decimal(9,4)")
            //  .HasComputedColumnSql(
            //      "CAST([Correct] * 1.0 / NULLIF([Answered],0) AS decimal(9,4))",
            //      stored: true);

            // Kategóriánkénti statisztikai lekérdezések
            b.HasIndex(x => x.CategoryId)
             .HasDatabaseName("IX_PlayerCategoryStats_CategoryId");

            // Helyességi arány szerinti rangsor
            b.HasIndex(x => x.Ratio)
             .HasDatabaseName("IX_PlayerCategoryStats_Ratio_DESC");
        }
    }
}

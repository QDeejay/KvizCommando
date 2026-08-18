using KvizCommando.Server.Domain.Entities.Statistics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KvizCommando.Server.Infrastructure.Persistence.Configurations
{
    public class PlayerAskStatsConfiguration : IEntityTypeConfiguration<PlayerAskStats>
    {
        /// <summary>
        /// Beállítja az entitás EF Core leképezését és adatbázis-korlátait.
        /// </summary>
        public void Configure(EntityTypeBuilder<PlayerAskStats> b)
        {
            b.ToTable("PlayerAskStats");

            b.HasKey(x => x.PlayerId);

            b.Property(x => x.TotalQuestionsAsked).IsRequired();
            b.Property(x => x.TotalAskPointsEarned).IsRequired();

            // SQLite számított oszlop az átlagpontszámhoz.
            b.Property(x => x.AveragePointsPerAsk)
             .HasColumnType("REAL")
             .HasComputedColumnSql(
                 "CASE WHEN [TotalQuestionsAsked] = 0 THEN 0.0 ELSE (1.0 * [TotalAskPointsEarned] / [TotalQuestionsAsked]) END",
                 stored: false);

            // SQL Server alternatíva
            // b.Property(x => x.AveragePointsPerAsk)
            //  .HasColumnType("decimal(9,2)")
            //  .HasComputedColumnSql(
            //      "CAST([TotalAskPointsEarned] * 1.0 / NULLIF([TotalQuestionsAsked],0) AS decimal(9,2))",
            //      stored: true);

            // Átlagpontszám szerinti rangsor
            b.HasIndex(x => x.AveragePointsPerAsk)
             .HasDatabaseName("IX_PlayerAskStats_AvgPoints_DESC");
        }
    }
}

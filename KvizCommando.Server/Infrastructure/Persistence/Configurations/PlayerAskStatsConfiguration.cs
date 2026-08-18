using KvizCommando.Server.Domain.Entities.Statistics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KvizCommando.Server.Infrastructure.Persistence.Configurations
{
    public class PlayerAskStatsConfiguration : IEntityTypeConfiguration<PlayerAskStats>
    {
        public void Configure(EntityTypeBuilder<PlayerAskStats> b)
        {
            b.ToTable("PlayerAskStats");

            b.HasKey(x => x.PlayerId);

            b.Property(x => x.TotalQuestionsAsked).IsRequired();
            b.Property(x => x.TotalAskPointsEarned).IsRequired();

            // Perzisztált számított oszlop az átlagra (2 tizedesre állítva)

            /// SQLite syntax
            b.Property(x => x.AveragePointsPerAsk)
             .HasColumnType("REAL")
             .HasComputedColumnSql(
                 "CASE WHEN [TotalQuestionsAsked] = 0 THEN 0.0 ELSE (1.0 * [TotalAskPointsEarned] / [TotalQuestionsAsked]) END",
                 stored: false);

            /// SQL Server syntax (későbbre meghagyva)
            // b.Property(x => x.AveragePointsPerAsk)
            //  .HasColumnType("decimal(9,2)")
            //  .HasComputedColumnSql(
            //      "CAST([TotalAskPointsEarned] * 1.0 / NULLIF([TotalQuestionsAsked],0) AS decimal(9,2))",
            //      stored: true);

            // Ha lesz "Top kérdező" rangsor
            b.HasIndex(x => x.AveragePointsPerAsk)
             .HasDatabaseName("IX_PlayerAskStats_AvgPoints_DESC");
        }
    }
}

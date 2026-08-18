using KvizCommando.Server.Domain.Entities.Players;
using KvizCommando.Server.Domain.Entities.Statistics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KvizCommando.Server.Infrastructure.Persistence.Configurations;

public sealed class TeamStatisticConfiguration :
    IEntityTypeConfiguration<TeamStatistic>
{
    public void Configure(EntityTypeBuilder<TeamStatistic> builder)
    {
        builder.ToTable("TeamStatistics", table =>
            table.HasCheckConstraint(
                "CK_TeamStatistics_RankedPlacementsJson_Valid",
                "json_valid([RankedPlacementsJson])"));

        builder.HasKey(statistic => statistic.PlayerId);
        builder.Property(statistic => statistic.PlayerId)
            .ValueGeneratedNever();

        builder.HasOne<Player>()
            .WithOne()
            .HasForeignKey<TeamStatistic>(statistic => statistic.PlayerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(statistic => statistic.RankedPlayed)
            .IsRequired();
        builder.Property(statistic => statistic.RankedWon)
            .IsRequired();
        builder.Property(statistic => statistic.RankedHighScore)
            .HasColumnType("REAL")
            .IsRequired();
        builder.Property(statistic => statistic.RankedHighScoreTime)
            .IsRequired();
        builder.Property(statistic => statistic.RankedGuessCount)
            .IsRequired();
        builder.Property(statistic => statistic.RankedGuessErrorTotal)
            .HasColumnType("REAL")
            .IsRequired();
        builder.Property(statistic => statistic.RankedGuessErrorRatio)
            .HasColumnType("REAL")
            .HasComputedColumnSql(
                "CASE WHEN [RankedGuessCount] = 0 THEN 0.0 ELSE (1.0 * [RankedGuessErrorTotal] / [RankedGuessCount]) END",
                stored: false);
        builder.Property(statistic => statistic.RankedPlacementsJson)
            .IsRequired()
            .HasColumnType("TEXT");

        builder.HasIndex(statistic => new
            {
                statistic.RankedHighScore,
                statistic.RankedHighScoreTime
            })
            .IsDescending(true, false)
            .HasDatabaseName(
                "IX_TeamStatistics_RankedHighScore_Time");

        builder.HasIndex(statistic => statistic.RankedGuessErrorRatio)
            .HasDatabaseName(
                "IX_TeamStatistics_RankedGuessErrorRatio");
    }
}

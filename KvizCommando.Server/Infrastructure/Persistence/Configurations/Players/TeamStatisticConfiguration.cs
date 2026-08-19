using KvizCommando.Server.Domain.Entities.Players;
using KvizCommando.Server.Domain.Entities.Statistics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KvizCommando.Server.Infrastructure.Persistence.Configurations;

public sealed class TeamStatisticConfiguration :
    IEntityTypeConfiguration<TeamStatistic>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<TeamStatistic> builder)
    {
        builder.ToTable("TeamStatistics");

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
            .IsRequired();
        builder.Property(statistic => statistic.RankedHighScoreTime)
            .IsRequired();
        builder.Property(statistic => statistic.RankedGuessCount)
            .IsRequired();
        builder.Property(statistic => statistic.RankedGuessErrorTotal)
            .IsRequired();
        builder.Property(statistic => statistic.RankedGuessErrorRatio);
        builder.Property(statistic => statistic.RankedPlacementsJson)
            .IsRequired();

        // A providerfüggő oszloptípusokat, JSON-ellenőrzést és számított
        // oszlopot a központi provider konfiguráció adja meg.

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

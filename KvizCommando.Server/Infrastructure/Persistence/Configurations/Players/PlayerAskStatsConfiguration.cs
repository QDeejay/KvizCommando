using KvizCommando.Server.Domain.Entities.Statistics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KvizCommando.Server.Infrastructure.Persistence.Configurations
{
    public class PlayerAskStatsConfiguration : IEntityTypeConfiguration<PlayerAskStats>
    {
        /// <inheritdoc />
        public void Configure(EntityTypeBuilder<PlayerAskStats> b)
        {
            b.ToTable("PlayerAskStats");

            b.HasKey(x => x.PlayerId);

            b.Property(x => x.TotalQuestionsAsked).IsRequired();
            b.Property(x => x.TotalAskPointsEarned).IsRequired();

            // A számított oszlop SQL-kifejezését a provider konfigurációja adja meg.

            // Átlagpontszám szerinti rangsor
            b.HasIndex(x => x.AveragePointsPerAsk)
             .HasDatabaseName("IX_PlayerAskStats_AvgPoints_DESC");
        }
    }
}

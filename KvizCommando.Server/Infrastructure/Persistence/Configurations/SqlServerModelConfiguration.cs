using KvizCommando.Server.Domain.Entities.Compliance;
using KvizCommando.Server.Domain.Entities.Players;
using KvizCommando.Server.Domain.Entities.Questions;
using KvizCommando.Server.Domain.Entities.Statistics;
using Microsoft.EntityFrameworkCore;

namespace KvizCommando.Server.Infrastructure.Persistence.Configurations;

/// <summary>
/// Az SQL Server által igényelt adattípusokat és SQL-kifejezéseket állítja be.
/// </summary>
internal static class SqlServerModelConfiguration
{
    internal static void ConfigureApplication(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PlayerCharacter>(builder =>
        {
            builder.Property(x => x.CharactersJson).HasColumnType("nvarchar(max)");
            builder.Property(x => x.CandidatesJson).HasColumnType("nvarchar(max)");
            builder.ToTable(table =>
            {
                table.HasCheckConstraint(
                    "CK_PlayerCharacters_CharactersJson_Valid",
                    "ISJSON([CharactersJson]) = 1");
                table.HasCheckConstraint(
                    "CK_PlayerCharacters_CandidatesJson_Valid",
                    "ISJSON([CandidatesJson]) = 1");
            });
        });

        modelBuilder.Entity<PlayerLoadout>(builder =>
        {
            builder.Property(x => x.FactorySlotsJson).HasColumnType("nvarchar(max)");
            builder.Property(x => x.UserSlotsJson).HasColumnType("nvarchar(max)");
            builder.Property(x => x.PendingSlotsJson).HasColumnType("nvarchar(max)");
            builder.ToTable(table =>
            {
                table.HasCheckConstraint(
                    "CK_PlayerLoadouts_FactorySlots_Json",
                    "ISJSON([FactorySlotsJson]) = 1");
                table.HasCheckConstraint(
                    "CK_PlayerLoadouts_UserSlots_Json",
                    "ISJSON([UserSlotsJson]) = 1");
                table.HasCheckConstraint(
                    "CK_PlayerLoadouts_PendingSlots_Json",
                    "ISJSON([PendingSlotsJson]) = 1");
            });
        });

        modelBuilder.Entity<PlayerAskStats>(builder =>
            builder.Property(x => x.AveragePointsPerAsk)
                .HasColumnType("decimal(9,2)")
                .HasComputedColumnSql(
                    "CAST(CASE WHEN [TotalQuestionsAsked] = 0 THEN 0.0 ELSE ([TotalAskPointsEarned] * 1.0 / [TotalQuestionsAsked]) END AS decimal(9,2))",
                    stored: true));

        modelBuilder.Entity<PlayerCategoryStat>(builder =>
            builder.Property(x => x.Ratio)
                .HasColumnType("decimal(9,4)")
                .HasComputedColumnSql(
                    "CAST(CASE WHEN [Answered] = 0 THEN 0.0 ELSE ([Correct] * 1.0 / [Answered]) END AS decimal(9,4))",
                    stored: true));

        modelBuilder.Entity<TeamStatistic>(builder =>
        {
            builder.Property(x => x.RankedGuessErrorTotal)
                .HasColumnType("decimal(18,4)");
            builder.Property(x => x.RankedGuessErrorRatio)
                .HasColumnType("decimal(18,4)")
                .HasComputedColumnSql(
                    "CAST(CASE WHEN [RankedGuessCount] = 0 THEN 0.0 ELSE ([RankedGuessErrorTotal] / [RankedGuessCount]) END AS decimal(18,4))",
                    stored: true);
            builder.Property(x => x.RankedPlacementsJson).HasColumnType("nvarchar(max)");
            builder.ToTable(table =>
                table.HasCheckConstraint(
                    "CK_TeamStatistics_RankedPlacementsJson_Valid",
                    "ISJSON([RankedPlacementsJson]) = 1"));
        });

        modelBuilder.Entity<Player>()
            .Property(x => x.RowVersion)
            .IsRowVersion();

        modelBuilder.Entity<TermsConsent>().ToTable(table =>
        {
            table.HasCheckConstraint(
                "CK_TermsConsents_UserAgentHash_Len",
                "UserAgentHash IS NULL OR DATALENGTH(UserAgentHash) = 32");
            table.HasCheckConstraint(
                "CK_TermsConsents_IpHash_Len",
                "IpHash IS NULL OR DATALENGTH(IpHash) = 32");
        });
    }

    internal static void ConfigureGame(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserQuestion>()
            .Property(x => x.Ratio)
            .HasComputedColumnSql(
                "CAST(CASE WHEN Ask > 0 THEN CAST(OkAnswer AS FLOAT) / CAST(Ask AS FLOAT) ELSE 0 END AS FLOAT)",
                stored: false);
    }
}

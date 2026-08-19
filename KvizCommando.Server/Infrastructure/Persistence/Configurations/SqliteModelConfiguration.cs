using KvizCommando.Server.Domain.Entities.Compliance;
using KvizCommando.Server.Domain.Entities.Players;
using KvizCommando.Server.Domain.Entities.Questions;
using KvizCommando.Server.Domain.Entities.Statistics;
using Microsoft.EntityFrameworkCore;

namespace KvizCommando.Server.Infrastructure.Persistence.Configurations;

/// <summary>
/// Az SQLite által igényelt adattípusokat és SQL-kifejezéseket állítja be.
/// </summary>
internal static class SqliteModelConfiguration
{
    internal static void ConfigureApplication(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PlayerCharacter>(builder =>
        {
            builder.Property(x => x.CharactersJson).HasColumnType("TEXT");
            builder.Property(x => x.CandidatesJson).HasColumnType("TEXT");
            builder.ToTable(table =>
            {
                table.HasCheckConstraint(
                    "CK_PlayerCharacters_CharactersJson_Valid",
                    "json_valid([CharactersJson])");
                table.HasCheckConstraint(
                    "CK_PlayerCharacters_CandidatesJson_Valid",
                    "json_valid([CandidatesJson])");
            });
        });

        modelBuilder.Entity<PlayerLoadout>(builder =>
        {
            builder.Property(x => x.FactorySlotsJson).HasColumnType("TEXT");
            builder.Property(x => x.UserSlotsJson).HasColumnType("TEXT");
            builder.Property(x => x.PendingSlotsJson).HasColumnType("TEXT");
            builder.ToTable(table =>
            {
                table.HasCheckConstraint(
                    "CK_PlayerLoadouts_FactorySlots_Json",
                    "json_valid([FactorySlotsJson])");
                table.HasCheckConstraint(
                    "CK_PlayerLoadouts_UserSlots_Json",
                    "json_valid([UserSlotsJson])");
                table.HasCheckConstraint(
                    "CK_PlayerLoadouts_PendingSlots_Json",
                    "json_valid([PendingSlotsJson])");
            });
        });

        modelBuilder.Entity<PlayerAskStats>(builder =>
            builder.Property(x => x.AveragePointsPerAsk)
                .HasColumnType("REAL")
                .HasComputedColumnSql(
                    "CASE WHEN [TotalQuestionsAsked] = 0 THEN 0.0 ELSE (1.0 * [TotalAskPointsEarned] / [TotalQuestionsAsked]) END",
                    stored: false));

        modelBuilder.Entity<PlayerCategoryStat>(builder =>
            builder.Property(x => x.Ratio)
                .HasColumnType("REAL")
                .HasComputedColumnSql(
                    "CASE WHEN [Answered] = 0 THEN 0.0 ELSE (1.0 * [Correct] / [Answered]) END",
                    stored: false));

        modelBuilder.Entity<TeamStatistic>(builder =>
        {
            builder.Property(x => x.RankedHighScore).HasColumnType("REAL");
            builder.Property(x => x.RankedGuessErrorTotal).HasColumnType("REAL");
            builder.Property(x => x.RankedGuessErrorRatio)
                .HasColumnType("REAL")
                .HasComputedColumnSql(
                    "CASE WHEN [RankedGuessCount] = 0 THEN 0.0 ELSE (1.0 * [RankedGuessErrorTotal] / [RankedGuessCount]) END",
                    stored: false);
            builder.Property(x => x.RankedPlacementsJson).HasColumnType("TEXT");
            builder.ToTable(table =>
                table.HasCheckConstraint(
                    "CK_TeamStatistics_RankedPlacementsJson_Valid",
                    "json_valid([RankedPlacementsJson])"));
        });

        modelBuilder.Entity<Player>()
            .Property(x => x.RowVersion)
            .IsConcurrencyToken();

        modelBuilder.Entity<TermsConsent>().ToTable(table =>
        {
            table.HasCheckConstraint(
                "CK_TermsConsents_UserAgentHash_Len",
                "UserAgentHash IS NULL OR length(UserAgentHash) = 32");
            table.HasCheckConstraint(
                "CK_TermsConsents_IpHash_Len",
                "IpHash IS NULL OR length(IpHash) = 32");
        });
    }

    internal static void ConfigureGame(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserQuestion>()
            .Property(x => x.Ratio)
            .HasComputedColumnSql(
                "CASE WHEN Ask > 0 THEN CAST(OkAnswer AS REAL) / CAST(Ask AS REAL) ELSE 0 END",
                stored: false);
    }
}

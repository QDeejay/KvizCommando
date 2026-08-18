using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KvizCommando.Server.Data.Migrations.Identity;

/// <inheritdoc />
public partial class AddRankedScoreCompensation : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_TeamStatistics_RankedHighScore_Time",
            table: "TeamStatistics");

        migrationBuilder.AlterColumn<double>(
            name: "RankedHighScore",
            table: "TeamStatistics",
            type: "REAL",
            nullable: false,
            oldClrType: typeof(int),
            oldType: "INTEGER");

        migrationBuilder.CreateIndex(
            name: "IX_TeamStatistics_RankedHighScore_Time",
            table: "TeamStatistics",
            columns:
            [
                "RankedHighScore",
                "RankedHighScoreTime"
            ],
            descending: [true, false]);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_TeamStatistics_RankedHighScore_Time",
            table: "TeamStatistics");

        migrationBuilder.AlterColumn<int>(
            name: "RankedHighScore",
            table: "TeamStatistics",
            type: "INTEGER",
            nullable: false,
            oldClrType: typeof(double),
            oldType: "REAL");

        migrationBuilder.CreateIndex(
            name: "IX_TeamStatistics_RankedHighScore_Time",
            table: "TeamStatistics",
            columns:
            [
                "RankedHighScore",
                "RankedHighScoreTime"
            ],
            descending: [true, false]);
    }
}

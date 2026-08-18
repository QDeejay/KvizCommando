using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KvizCommando.Server.Data.Migrations.Identity
{
    /// <inheritdoc />
    public partial class AddRankedGuessStatistics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RankedGuessCount",
                table: "TeamStatistics",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "RankedGuessErrorTotal",
                table: "TeamStatistics",
                type: "REAL",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "RankedGuessErrorRatio",
                table: "TeamStatistics",
                type: "REAL",
                nullable: false,
                computedColumnSql: "CASE WHEN [RankedGuessCount] = 0 THEN 0.0 ELSE (1.0 * [RankedGuessErrorTotal] / [RankedGuessCount]) END",
                stored: false);

            migrationBuilder.CreateIndex(
                name: "IX_TeamStatistics_RankedGuessErrorRatio",
                table: "TeamStatistics",
                column: "RankedGuessErrorRatio");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TeamStatistics_RankedGuessErrorRatio",
                table: "TeamStatistics");

            migrationBuilder.DropColumn(
                name: "RankedGuessErrorRatio",
                table: "TeamStatistics");

            migrationBuilder.DropColumn(
                name: "RankedGuessCount",
                table: "TeamStatistics");

            migrationBuilder.DropColumn(
                name: "RankedGuessErrorTotal",
                table: "TeamStatistics");
        }
    }
}

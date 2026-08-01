using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KvizCommando.Server.Data.Migrations.Identity
{
    /// <inheritdoc />
    public partial class AddTeamStatistics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TeamStatistics",
                columns: table => new
                {
                    PlayerId = table.Column<int>(
                        type: "INTEGER",
                        nullable: false),
                    RankedPlayed = table.Column<int>(
                        type: "INTEGER",
                        nullable: false),
                    RankedWon = table.Column<int>(
                        type: "INTEGER",
                        nullable: false),
                    RankedHighScore = table.Column<int>(
                        type: "INTEGER",
                        nullable: false),
                    RankedHighScoreTime = table.Column<double>(
                        type: "REAL",
                        nullable: false),
                    RankedPlacementsJson = table.Column<string>(
                        type: "TEXT",
                        nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey(
                        "PK_TeamStatistics",
                        x => x.PlayerId);
                    table.CheckConstraint(
                        "CK_TeamStatistics_RankedPlacementsJson_Valid",
                        "json_valid([RankedPlacementsJson])");
                    table.ForeignKey(
                        name: "FK_TeamStatistics_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "PlayerId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TeamStatistics_RankedHighScore_Time",
                table: "TeamStatistics",
                columns: new[]
                {
                    "RankedHighScore",
                    "RankedHighScoreTime"
                },
                descending: new[] { true, false });

            migrationBuilder.Sql(
                """
                INSERT INTO "TeamStatistics" (
                    "PlayerId",
                    "RankedPlayed",
                    "RankedWon",
                    "RankedHighScore",
                    "RankedHighScoreTime",
                    "RankedPlacementsJson")
                SELECT
                    "PlayerId",
                    0,
                    0,
                    0,
                    0,
                    '{"Players2":[0,0],"Players3":[0,0,0],"Players4":[0,0,0,0]}'
                FROM "Players";
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TeamStatistics");
        }
    }
}

/**
 * ÚJ FÁJL: szabályos EF Core migrációként létrehozza a
 * TeamStatistics táblát, a highscore indexet és a Players
 * idegen kulcsot, majd nullázott alapstatisztikával visszatölti a
 * már létező játékosokat. Telepítése az egyszeri
 * Update-Database -Context ApplicationDbContext paranccsal történik.
 */

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KvizCommando.Server.Data.Migrations.Identity
{
    /// <inheritdoc />
    public partial class InsertOrientstatistics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PlayerOrientStat",
                columns: table => new
                {
                    PlayerId = table.Column<int>(type: "INTEGER", nullable: false),
                    OrientId = table.Column<short>(type: "smallint", nullable: false),
                    HighScore = table.Column<int>(type: "INTEGER", nullable: false),
                    HighScoreTime = table.Column<double>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerOrientStat", x => new { x.PlayerId, x.OrientId });
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlayerOrientStat_CategoryId",
                table: "PlayerOrientStat",
                column: "OrientId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlayerOrientStat");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KvizCommando.Server.Data.Migrations.Identity
{
    /// <inheritdoc />
    public partial class InsertCategoryStatistics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "HighScore",
                table: "PlayerCategoryStats",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "HighScoreTime",
                table: "PlayerCategoryStats",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HighScore",
                table: "PlayerCategoryStats");

            migrationBuilder.DropColumn(
                name: "HighScoreTime",
                table: "PlayerCategoryStats");
        }
    }
}

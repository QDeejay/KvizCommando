using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KvizCommando.Server.Data.Migrations.SqlServer.Application
{
    /// <inheritdoc />
    public partial class CascadeDeletionUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddForeignKey(
                name: "FK_PlayerAskStats_Players_PlayerId",
                table: "PlayerAskStats",
                column: "PlayerId",
                principalTable: "Players",
                principalColumn: "PlayerId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PlayerCategoryStats_Players_PlayerId",
                table: "PlayerCategoryStats",
                column: "PlayerId",
                principalTable: "Players",
                principalColumn: "PlayerId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PlayerCharacters_Players_PlayerId",
                table: "PlayerCharacters",
                column: "PlayerId",
                principalTable: "Players",
                principalColumn: "PlayerId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PlayerLoadouts_Players_PlayerId",
                table: "PlayerLoadouts",
                column: "PlayerId",
                principalTable: "Players",
                principalColumn: "PlayerId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PlayerOrientStat_Players_PlayerId",
                table: "PlayerOrientStat",
                column: "PlayerId",
                principalTable: "Players",
                principalColumn: "PlayerId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PlayerAskStats_Players_PlayerId",
                table: "PlayerAskStats");

            migrationBuilder.DropForeignKey(
                name: "FK_PlayerCategoryStats_Players_PlayerId",
                table: "PlayerCategoryStats");

            migrationBuilder.DropForeignKey(
                name: "FK_PlayerCharacters_Players_PlayerId",
                table: "PlayerCharacters");

            migrationBuilder.DropForeignKey(
                name: "FK_PlayerLoadouts_Players_PlayerId",
                table: "PlayerLoadouts");

            migrationBuilder.DropForeignKey(
                name: "FK_PlayerOrientStat_Players_PlayerId",
                table: "PlayerOrientStat");
        }
    }
}

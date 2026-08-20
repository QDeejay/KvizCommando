using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KvizCommando.Server.Data.Migrations.SqlServer.Application
{
    /// <inheritdoc />
    public partial class ProfileSchemaSqlServerApplication : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "TeamName",
                table: "Players",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(128)",
                oldMaxLength: 128);

            migrationBuilder.AddColumn<string>(
                name: "CaptainAvatar",
                table: "Players",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NormalizedTeamName",
                table: "Players",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "TeamNameChangedUtc",
                table: "Players",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "UX_Players_NormalizedTeamName",
                table: "Players",
                column: "NormalizedTeamName",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UX_Players_NormalizedTeamName",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "CaptainAvatar",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "NormalizedTeamName",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "TeamNameChangedUtc",
                table: "Players");

            migrationBuilder.AlterColumn<string>(
                name: "TeamName",
                table: "Players",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);
        }
    }
}

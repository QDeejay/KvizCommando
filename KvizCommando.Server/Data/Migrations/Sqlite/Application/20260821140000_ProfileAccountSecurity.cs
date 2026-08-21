using KvizCommando.Server.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KvizCommando.Server.Data.Migrations.Sqlite.Application;

[DbContext(typeof(SqliteApplicationDbContext))]
[Migration("20260821140000_ProfileAccountSecurity")]
public sealed class ProfileAccountSecurity : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "RegistrationBenefitClaims",
            columns: table => new
            {
                EmailFingerprint = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                EligibleAgainAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_RegistrationBenefitClaims", x => x.EmailFingerprint));
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder) =>
        migrationBuilder.DropTable(name: "RegistrationBenefitClaims");
}

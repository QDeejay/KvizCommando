using KvizCommando.Server.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KvizCommando.Server.Data.Migrations.SqlServer.Application;

[DbContext(typeof(SqlServerApplicationDbContext))]
[Migration("20260821140100_ProfileAccountSecuritySqlServer")]
public sealed class ProfileAccountSecuritySqlServer : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "RegistrationBenefitClaims",
            columns: table => new
            {
                EmailFingerprint = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                EligibleAgainAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_RegistrationBenefitClaims", x => x.EmailFingerprint));
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder) =>
        migrationBuilder.DropTable(name: "RegistrationBenefitClaims");
}

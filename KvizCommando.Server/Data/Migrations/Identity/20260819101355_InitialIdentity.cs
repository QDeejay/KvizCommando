using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KvizCommando.Server.Data.Migrations.Identity
{
    /// <inheritdoc />
    public partial class InitialIdentity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    DisplayName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    NormalizedDisplayName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    PreferredLocale = table.Column<string>(type: "TEXT", maxLength: 16, nullable: false, defaultValue: "hu-HU"),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    AcceptTerms = table.Column<bool>(type: "INTEGER", nullable: false),
                    MarketingConsent = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    UserName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "INTEGER", nullable: false),
                    PasswordHash = table.Column<string>(type: "TEXT", nullable: true),
                    SecurityStamp = table.Column<string>(type: "TEXT", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "TEXT", nullable: true),
                    PhoneNumber = table.Column<string>(type: "TEXT", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "INTEGER", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MarketingConsents",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    Granted = table.Column<bool>(type: "INTEGER", nullable: false),
                    ChangedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MarketingConsents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PendingQuestion",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Status = table.Column<string>(type: "TEXT", nullable: false),
                    Remark = table.Column<string>(type: "TEXT", nullable: true),
                    SubmittedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    PlayerId = table.Column<int>(type: "INTEGER", nullable: false),
                    Question = table.Column<string>(type: "TEXT", nullable: false),
                    CategoryNo = table.Column<int>(type: "INTEGER", nullable: false),
                    AnswersJson = table.Column<string>(type: "TEXT", nullable: false),
                    Reported = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PendingQuestion", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PlayerAskStats",
                columns: table => new
                {
                    PlayerId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TotalQuestionsAsked = table.Column<int>(type: "INTEGER", nullable: false),
                    TotalAskPointsEarned = table.Column<int>(type: "INTEGER", nullable: false),
                    AveragePointsPerAsk = table.Column<decimal>(type: "REAL", nullable: false, computedColumnSql: "CASE WHEN [TotalQuestionsAsked] = 0 THEN 0.0 ELSE (1.0 * [TotalAskPointsEarned] / [TotalQuestionsAsked]) END", stored: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerAskStats", x => x.PlayerId);
                });

            migrationBuilder.CreateTable(
                name: "PlayerCategoryStats",
                columns: table => new
                {
                    PlayerId = table.Column<int>(type: "INTEGER", nullable: false),
                    CategoryId = table.Column<short>(type: "smallint", nullable: false),
                    Answered = table.Column<int>(type: "INTEGER", nullable: false),
                    Correct = table.Column<int>(type: "INTEGER", nullable: false),
                    HighScore = table.Column<int>(type: "INTEGER", nullable: false),
                    HighScoreTime = table.Column<double>(type: "REAL", nullable: false),
                    Ratio = table.Column<decimal>(type: "REAL", nullable: false, computedColumnSql: "CASE WHEN [Answered] = 0 THEN 0.0 ELSE (1.0 * [Correct] / [Answered]) END", stored: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerCategoryStats", x => new { x.PlayerId, x.CategoryId });
                });

            migrationBuilder.CreateTable(
                name: "PlayerCharacters",
                columns: table => new
                {
                    PlayerId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CharactersJson = table.Column<string>(type: "TEXT", nullable: false),
                    CandidatesJson = table.Column<string>(type: "TEXT", nullable: false),
                    UpdatedUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerCharacters", x => x.PlayerId);
                    table.CheckConstraint("CK_PlayerCharacters_CandidatesJson_Valid", "json_valid([CandidatesJson])");
                    table.CheckConstraint("CK_PlayerCharacters_CharactersJson_Valid", "json_valid([CharactersJson])");
                });

            migrationBuilder.CreateTable(
                name: "PlayerLoadouts",
                columns: table => new
                {
                    PlayerId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FactorySlotsJson = table.Column<string>(type: "TEXT", nullable: false),
                    UserSlotsJson = table.Column<string>(type: "TEXT", nullable: false),
                    PendingSlotsJson = table.Column<string>(type: "TEXT", nullable: false),
                    HelpLevelsJson = table.Column<string>(type: "TEXT", nullable: false),
                    UpdatedUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerLoadouts", x => x.PlayerId);
                    table.CheckConstraint("CK_PlayerLoadouts_FactorySlots_Json", "json_valid([FactorySlotsJson])");
                    table.CheckConstraint("CK_PlayerLoadouts_PendingSlots_Json", "json_valid([PendingSlotsJson])");
                    table.CheckConstraint("CK_PlayerLoadouts_UserSlots_Json", "json_valid([UserSlotsJson])");
                });

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

            migrationBuilder.CreateTable(
                name: "Players",
                columns: table => new
                {
                    PlayerId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    DisplayName = table.Column<string>(type: "TEXT", nullable: false),
                    TeamName = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    RankEnum = table.Column<int>(type: "INTEGER", nullable: false),
                    XP = table.Column<int>(type: "INTEGER", nullable: false),
                    Credit = table.Column<int>(type: "INTEGER", nullable: false),
                    DevPoint = table.Column<int>(type: "INTEGER", nullable: false),
                    Voucher = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Players", x => x.PlayerId);
                });

            migrationBuilder.CreateTable(
                name: "TermsConsents",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    TermsVersion = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    AcceptedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UserAgentHash = table.Column<byte[]>(type: "BLOB", maxLength: 32, nullable: true),
                    IpHash = table.Column<byte[]>(type: "BLOB", maxLength: 32, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TermsConsents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserPaymentMethods",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    Processor = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    PaymentMethodToken = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    CardBrand = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    CardLast4 = table.Column<string>(type: "TEXT", maxLength: 4, nullable: false),
                    ExpMonth = table.Column<int>(type: "INTEGER", nullable: false),
                    ExpYear = table.Column<int>(type: "INTEGER", nullable: false),
                    IsDefault = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPaymentMethods", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserPii",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    EmailEncrypted = table.Column<byte[]>(type: "BLOB", nullable: true),
                    EmailNonce = table.Column<byte[]>(type: "BLOB", nullable: true),
                    EmailTag = table.Column<byte[]>(type: "BLOB", nullable: true),
                    EmailNormHash = table.Column<byte[]>(type: "BLOB", nullable: true),
                    PhoneEncrypted = table.Column<byte[]>(type: "BLOB", nullable: true),
                    PhoneNonce = table.Column<byte[]>(type: "BLOB", nullable: true),
                    PhoneTag = table.Column<byte[]>(type: "BLOB", nullable: true),
                    PhoneNormHash = table.Column<byte[]>(type: "BLOB", nullable: true),
                    BillingNameEncrypted = table.Column<byte[]>(type: "BLOB", nullable: true),
                    BillingNameNonce = table.Column<byte[]>(type: "BLOB", nullable: true),
                    BillingNameTag = table.Column<byte[]>(type: "BLOB", nullable: true),
                    BillingAddressEncrypted = table.Column<byte[]>(type: "BLOB", nullable: true),
                    BillingAddressNonce = table.Column<byte[]>(type: "BLOB", nullable: true),
                    BillingAddressTag = table.Column<byte[]>(type: "BLOB", nullable: true),
                    CreatedUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPii", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "UserQuestion",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Ask = table.Column<int>(type: "INTEGER", nullable: false),
                    OkAnswer = table.Column<int>(type: "INTEGER", nullable: false),
                    Ratio = table.Column<double>(type: "REAL", nullable: false, computedColumnSql: "CASE WHEN Ask > 0 THEN CAST(OkAnswer AS REAL) / CAST(Ask AS REAL) ELSE 0 END", stored: false),
                    PlayerId = table.Column<int>(type: "INTEGER", nullable: false),
                    Question = table.Column<string>(type: "TEXT", nullable: false),
                    CategoryNo = table.Column<int>(type: "INTEGER", nullable: false),
                    AnswersJson = table.Column<string>(type: "TEXT", nullable: false),
                    Reported = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserQuestion", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RoleId = table.Column<string>(type: "TEXT", nullable: false),
                    ClaimType = table.Column<string>(type: "TEXT", nullable: true),
                    ClaimValue = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    ClaimType = table.Column<string>(type: "TEXT", nullable: true),
                    ClaimValue = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "TEXT", nullable: false),
                    ProviderKey = table.Column<string>(type: "TEXT", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "TEXT", nullable: true),
                    UserId = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    RoleId = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    LoginProvider = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    Value = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TeamStatistics",
                columns: table => new
                {
                    PlayerId = table.Column<int>(type: "INTEGER", nullable: false),
                    RankedPlayed = table.Column<int>(type: "INTEGER", nullable: false),
                    RankedWon = table.Column<int>(type: "INTEGER", nullable: false),
                    RankedHighScore = table.Column<double>(type: "REAL", nullable: false),
                    RankedHighScoreTime = table.Column<double>(type: "REAL", nullable: false),
                    RankedGuessCount = table.Column<int>(type: "INTEGER", nullable: false),
                    RankedGuessErrorTotal = table.Column<decimal>(type: "REAL", nullable: false),
                    RankedGuessErrorRatio = table.Column<decimal>(type: "REAL", nullable: false, computedColumnSql: "CASE WHEN [RankedGuessCount] = 0 THEN 0.0 ELSE (1.0 * [RankedGuessErrorTotal] / [RankedGuessCount]) END", stored: false),
                    RankedPlacementsJson = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeamStatistics", x => x.PlayerId);
                    table.CheckConstraint("CK_TeamStatistics_RankedPlacementsJson_Valid", "json_valid([RankedPlacementsJson])");
                    table.ForeignKey(
                        name: "FK_TeamStatistics_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "PlayerId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_AspNetUsers_NormalizedDisplayName_Active",
                table: "AspNetUsers",
                column: "NormalizedDisplayName",
                unique: true,
                filter: "[NormalizedDisplayName] IS NOT NULL AND [IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserTokens_ExpiresAt",
                table: "AspNetUserTokens",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_MarketingConsents_UserId_ChangedAtUtc",
                table: "MarketingConsents",
                columns: new[] { "UserId", "ChangedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_PendingQuestion_PlayerId",
                table: "PendingQuestion",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerAskStats_AvgPoints_DESC",
                table: "PlayerAskStats",
                column: "AveragePointsPerAsk");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerCategoryStats_CategoryId",
                table: "PlayerCategoryStats",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerCategoryStats_Ratio_DESC",
                table: "PlayerCategoryStats",
                column: "Ratio");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerOrientStat_CategoryId",
                table: "PlayerOrientStat",
                column: "OrientId");

            migrationBuilder.CreateIndex(
                name: "IX_Players_Credit",
                table: "Players",
                column: "Credit");

            migrationBuilder.CreateIndex(
                name: "IX_Players_XP",
                table: "Players",
                column: "XP");

            migrationBuilder.CreateIndex(
                name: "UX_Players_UserId",
                table: "Players",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TeamStatistics_RankedGuessErrorRatio",
                table: "TeamStatistics",
                column: "RankedGuessErrorRatio");

            migrationBuilder.CreateIndex(
                name: "IX_TeamStatistics_RankedHighScore_Time",
                table: "TeamStatistics",
                columns: new[] { "RankedHighScore", "RankedHighScoreTime" },
                descending: new[] { true, false });

            migrationBuilder.CreateIndex(
                name: "IX_TermsConsents_UserId_AcceptedAtUtc",
                table: "TermsConsents",
                columns: new[] { "UserId", "AcceptedAtUtc" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "UX_TermsConsents_UserId_TermsVersion",
                table: "TermsConsents",
                columns: new[] { "UserId", "TermsVersion" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserPaymentMethods_User_IsDefault",
                table: "UserPaymentMethods",
                columns: new[] { "UserId", "IsDefault" });

            migrationBuilder.CreateIndex(
                name: "IX_UserPii_PhoneNormHash",
                table: "UserPii",
                column: "PhoneNormHash");

            migrationBuilder.CreateIndex(
                name: "UX_UserPii_EmailNormHash",
                table: "UserPii",
                column: "EmailNormHash",
                unique: true,
                filter: "[EmailNormHash] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_UserQuestion_PlayerId",
                table: "UserQuestion",
                column: "PlayerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "MarketingConsents");

            migrationBuilder.DropTable(
                name: "PendingQuestion");

            migrationBuilder.DropTable(
                name: "PlayerAskStats");

            migrationBuilder.DropTable(
                name: "PlayerCategoryStats");

            migrationBuilder.DropTable(
                name: "PlayerCharacters");

            migrationBuilder.DropTable(
                name: "PlayerLoadouts");

            migrationBuilder.DropTable(
                name: "PlayerOrientStat");

            migrationBuilder.DropTable(
                name: "TeamStatistics");

            migrationBuilder.DropTable(
                name: "TermsConsents");

            migrationBuilder.DropTable(
                name: "UserPaymentMethods");

            migrationBuilder.DropTable(
                name: "UserPii");

            migrationBuilder.DropTable(
                name: "UserQuestion");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "Players");
        }
    }
}

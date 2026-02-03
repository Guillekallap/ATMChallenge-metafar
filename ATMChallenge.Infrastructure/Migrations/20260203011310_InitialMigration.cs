using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ATMChallenge.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Accounts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AccountNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Balance = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    LastWithdrawal = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Accounts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Cards",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CardNumber = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    PinHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsBlocked = table.Column<bool>(type: "bit", nullable: false),
                    FailedPinAttempts = table.Column<int>(type: "int", nullable: false),
                    AccountId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cards", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Cards_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Operations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AccountId = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Operations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Operations_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FullName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AccountId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Accounts",
                columns: new[] { "Id", "AccountNumber", "Balance", "CreatedAt", "LastWithdrawal", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, "AR12345678", 10000m, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 30, 14, 0, 0, 0, DateTimeKind.Utc), null },
                    { 2, "AR87654321", 25000m, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 29, 10, 0, 0, 0, DateTimeKind.Utc), null },
                    { 3, "AR11223344", 5500m, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 26, 16, 0, 0, 0, DateTimeKind.Utc), null },
                    { 4, "AR55667788", 50000m, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 31, 9, 0, 0, 0, DateTimeKind.Utc), null },
                    { 5, "AR99887766", 15750m, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 28, 11, 0, 0, 0, DateTimeKind.Utc), null }
                });

            migrationBuilder.InsertData(
                table: "Cards",
                columns: new[] { "Id", "AccountId", "CardNumber", "CreatedAt", "FailedPinAttempts", "IsBlocked", "PinHash", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, 1, "4000000000000001", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 0, false, "$2a$11$TyFAoaAeToGuenFFXqShVOrPc1rYjodYKm58dF6TlRUz8lEu5CA5a", null },
                    { 2, 2, "4000000000000002", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 0, false, "$2a$11$LUCtFqUXof3W7HuPJZcE2uLwzfxKijgFvSoF3SBNF8wypQNCfBjn.", null },
                    { 3, 3, "4000000000000003", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 0, false, "$2a$11$/d7bbpmyYubWmXhVmAoJjuq6zKYd.VOpxUpxBMAcsiQHMA81bHJgq", null },
                    { 4, 4, "4000000000000004", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 0, false, "$2a$11$Z3d5Id2MUipQUH7vDRqrD.NZArJuMUaSo2F2cHAI1Kj6OdQM2nYB2", null },
                    { 5, 5, "4000000000000005", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 0, false, "$2a$11$Lug9xGuT./EImIoBuw2R3eveCjI3PIZCDOyDrSwidDsdBT1NGTTli", null }
                });

            migrationBuilder.InsertData(
                table: "Operations",
                columns: new[] { "Id", "AccountId", "Amount", "CreatedAt", "CreatedBy", "Timestamp", "Type" },
                values: new object[,]
                {
                    { 1, 1, 500m, new DateTime(2026, 1, 2, 10, 0, 0, 0, DateTimeKind.Utc), "4000000000000001", new DateTime(2026, 1, 2, 10, 0, 0, 0, DateTimeKind.Utc), 0 },
                    { 2, 1, 1000m, new DateTime(2026, 1, 4, 10, 0, 0, 0, DateTimeKind.Utc), "4000000000000001", new DateTime(2026, 1, 4, 10, 0, 0, 0, DateTimeKind.Utc), 0 },
                    { 3, 1, 250m, new DateTime(2026, 1, 6, 10, 0, 0, 0, DateTimeKind.Utc), "4000000000000001", new DateTime(2026, 1, 6, 10, 0, 0, 0, DateTimeKind.Utc), 0 },
                    { 4, 1, 750m, new DateTime(2026, 1, 8, 10, 0, 0, 0, DateTimeKind.Utc), "4000000000000001", new DateTime(2026, 1, 8, 10, 0, 0, 0, DateTimeKind.Utc), 0 },
                    { 5, 1, 300m, new DateTime(2026, 1, 10, 10, 0, 0, 0, DateTimeKind.Utc), "4000000000000001", new DateTime(2026, 1, 10, 10, 0, 0, 0, DateTimeKind.Utc), 0 },
                    { 6, 1, 1500m, new DateTime(2026, 1, 12, 10, 0, 0, 0, DateTimeKind.Utc), "4000000000000001", new DateTime(2026, 1, 12, 10, 0, 0, 0, DateTimeKind.Utc), 0 },
                    { 7, 1, 200m, new DateTime(2026, 1, 14, 10, 0, 0, 0, DateTimeKind.Utc), "4000000000000001", new DateTime(2026, 1, 14, 10, 0, 0, 0, DateTimeKind.Utc), 0 },
                    { 8, 1, 800m, new DateTime(2026, 1, 16, 10, 0, 0, 0, DateTimeKind.Utc), "4000000000000001", new DateTime(2026, 1, 16, 10, 0, 0, 0, DateTimeKind.Utc), 0 },
                    { 9, 1, 450m, new DateTime(2026, 1, 18, 10, 0, 0, 0, DateTimeKind.Utc), "4000000000000001", new DateTime(2026, 1, 18, 10, 0, 0, 0, DateTimeKind.Utc), 0 },
                    { 10, 1, 1200m, new DateTime(2026, 1, 20, 10, 0, 0, 0, DateTimeKind.Utc), "4000000000000001", new DateTime(2026, 1, 20, 10, 0, 0, 0, DateTimeKind.Utc), 0 },
                    { 11, 1, 350m, new DateTime(2026, 1, 22, 10, 0, 0, 0, DateTimeKind.Utc), "4000000000000001", new DateTime(2026, 1, 22, 10, 0, 0, 0, DateTimeKind.Utc), 0 },
                    { 12, 1, 600m, new DateTime(2026, 1, 24, 10, 0, 0, 0, DateTimeKind.Utc), "4000000000000001", new DateTime(2026, 1, 24, 10, 0, 0, 0, DateTimeKind.Utc), 0 },
                    { 13, 1, 900m, new DateTime(2026, 1, 26, 10, 0, 0, 0, DateTimeKind.Utc), "4000000000000001", new DateTime(2026, 1, 26, 10, 0, 0, 0, DateTimeKind.Utc), 0 },
                    { 14, 1, 400m, new DateTime(2026, 1, 28, 10, 0, 0, 0, DateTimeKind.Utc), "4000000000000001", new DateTime(2026, 1, 28, 10, 0, 0, 0, DateTimeKind.Utc), 0 },
                    { 15, 1, 550m, new DateTime(2026, 1, 30, 10, 0, 0, 0, DateTimeKind.Utc), "4000000000000001", new DateTime(2026, 1, 30, 10, 0, 0, 0, DateTimeKind.Utc), 0 },
                    { 16, 2, 2000m, new DateTime(2026, 1, 3, 14, 0, 0, 0, DateTimeKind.Utc), "4000000000000002", new DateTime(2026, 1, 3, 14, 0, 0, 0, DateTimeKind.Utc), 0 },
                    { 17, 2, 1500m, new DateTime(2026, 1, 5, 14, 0, 0, 0, DateTimeKind.Utc), "4000000000000002", new DateTime(2026, 1, 5, 14, 0, 0, 0, DateTimeKind.Utc), 0 },
                    { 18, 2, 3000m, new DateTime(2026, 1, 7, 14, 0, 0, 0, DateTimeKind.Utc), "4000000000000002", new DateTime(2026, 1, 7, 14, 0, 0, 0, DateTimeKind.Utc), 0 },
                    { 19, 2, 500m, new DateTime(2026, 1, 9, 14, 0, 0, 0, DateTimeKind.Utc), "4000000000000002", new DateTime(2026, 1, 9, 14, 0, 0, 0, DateTimeKind.Utc), 0 },
                    { 20, 2, 2500m, new DateTime(2026, 1, 11, 14, 0, 0, 0, DateTimeKind.Utc), "4000000000000002", new DateTime(2026, 1, 11, 14, 0, 0, 0, DateTimeKind.Utc), 0 },
                    { 21, 2, 1000m, new DateTime(2026, 1, 13, 14, 0, 0, 0, DateTimeKind.Utc), "4000000000000002", new DateTime(2026, 1, 13, 14, 0, 0, 0, DateTimeKind.Utc), 0 },
                    { 22, 2, 1800m, new DateTime(2026, 1, 15, 14, 0, 0, 0, DateTimeKind.Utc), "4000000000000002", new DateTime(2026, 1, 15, 14, 0, 0, 0, DateTimeKind.Utc), 0 },
                    { 23, 2, 700m, new DateTime(2026, 1, 17, 14, 0, 0, 0, DateTimeKind.Utc), "4000000000000002", new DateTime(2026, 1, 17, 14, 0, 0, 0, DateTimeKind.Utc), 0 },
                    { 24, 2, 2200m, new DateTime(2026, 1, 19, 14, 0, 0, 0, DateTimeKind.Utc), "4000000000000002", new DateTime(2026, 1, 19, 14, 0, 0, 0, DateTimeKind.Utc), 0 },
                    { 25, 2, 1300m, new DateTime(2026, 1, 23, 14, 0, 0, 0, DateTimeKind.Utc), "4000000000000002", new DateTime(2026, 1, 23, 14, 0, 0, 0, DateTimeKind.Utc), 0 },
                    { 26, 2, 900m, new DateTime(2026, 1, 27, 14, 0, 0, 0, DateTimeKind.Utc), "4000000000000002", new DateTime(2026, 1, 27, 14, 0, 0, 0, DateTimeKind.Utc), 0 },
                    { 27, 2, 1100m, new DateTime(2026, 1, 29, 14, 0, 0, 0, DateTimeKind.Utc), "4000000000000002", new DateTime(2026, 1, 29, 14, 0, 0, 0, DateTimeKind.Utc), 0 },
                    { 28, 3, 300m, new DateTime(2026, 1, 4, 16, 0, 0, 0, DateTimeKind.Utc), "4000000000000003", new DateTime(2026, 1, 4, 16, 0, 0, 0, DateTimeKind.Utc), 0 },
                    { 29, 3, 450m, new DateTime(2026, 1, 7, 16, 0, 0, 0, DateTimeKind.Utc), "4000000000000003", new DateTime(2026, 1, 7, 16, 0, 0, 0, DateTimeKind.Utc), 0 },
                    { 30, 3, 600m, new DateTime(2026, 1, 10, 16, 0, 0, 0, DateTimeKind.Utc), "4000000000000003", new DateTime(2026, 1, 10, 16, 0, 0, 0, DateTimeKind.Utc), 0 },
                    { 31, 3, 200m, new DateTime(2026, 1, 13, 16, 0, 0, 0, DateTimeKind.Utc), "4000000000000003", new DateTime(2026, 1, 13, 16, 0, 0, 0, DateTimeKind.Utc), 0 },
                    { 32, 3, 800m, new DateTime(2026, 1, 16, 16, 0, 0, 0, DateTimeKind.Utc), "4000000000000003", new DateTime(2026, 1, 16, 16, 0, 0, 0, DateTimeKind.Utc), 0 },
                    { 33, 3, 350m, new DateTime(2026, 1, 19, 16, 0, 0, 0, DateTimeKind.Utc), "4000000000000003", new DateTime(2026, 1, 19, 16, 0, 0, 0, DateTimeKind.Utc), 0 },
                    { 34, 3, 500m, new DateTime(2026, 1, 23, 16, 0, 0, 0, DateTimeKind.Utc), "4000000000000003", new DateTime(2026, 1, 23, 16, 0, 0, 0, DateTimeKind.Utc), 0 },
                    { 35, 3, 400m, new DateTime(2026, 1, 26, 16, 0, 0, 0, DateTimeKind.Utc), "4000000000000003", new DateTime(2026, 1, 26, 16, 0, 0, 0, DateTimeKind.Utc), 0 },
                    { 36, 4, 5000m, new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Utc), "4000000000000004", new DateTime(2026, 1, 3, 9, 0, 0, 0, DateTimeKind.Utc), 0 },
                    { 37, 4, 3500m, new DateTime(2026, 1, 6, 9, 0, 0, 0, DateTimeKind.Utc), "4000000000000004", new DateTime(2026, 1, 6, 9, 0, 0, 0, DateTimeKind.Utc), 0 },
                    { 38, 4, 4200m, new DateTime(2026, 1, 9, 9, 0, 0, 0, DateTimeKind.Utc), "4000000000000004", new DateTime(2026, 1, 9, 9, 0, 0, 0, DateTimeKind.Utc), 0 },
                    { 39, 4, 2800m, new DateTime(2026, 1, 12, 9, 0, 0, 0, DateTimeKind.Utc), "4000000000000004", new DateTime(2026, 1, 12, 9, 0, 0, 0, DateTimeKind.Utc), 0 },
                    { 40, 4, 6000m, new DateTime(2026, 1, 15, 9, 0, 0, 0, DateTimeKind.Utc), "4000000000000004", new DateTime(2026, 1, 15, 9, 0, 0, 0, DateTimeKind.Utc), 0 },
                    { 41, 4, 1500m, new DateTime(2026, 1, 18, 9, 0, 0, 0, DateTimeKind.Utc), "4000000000000004", new DateTime(2026, 1, 18, 9, 0, 0, 0, DateTimeKind.Utc), 0 },
                    { 42, 4, 3000m, new DateTime(2026, 1, 21, 9, 0, 0, 0, DateTimeKind.Utc), "4000000000000004", new DateTime(2026, 1, 21, 9, 0, 0, 0, DateTimeKind.Utc), 0 },
                    { 43, 4, 4500m, new DateTime(2026, 1, 24, 9, 0, 0, 0, DateTimeKind.Utc), "4000000000000004", new DateTime(2026, 1, 24, 9, 0, 0, 0, DateTimeKind.Utc), 0 },
                    { 44, 4, 2000m, new DateTime(2026, 1, 27, 9, 0, 0, 0, DateTimeKind.Utc), "4000000000000004", new DateTime(2026, 1, 27, 9, 0, 0, 0, DateTimeKind.Utc), 0 },
                    { 45, 4, 3800m, new DateTime(2026, 1, 31, 9, 0, 0, 0, DateTimeKind.Utc), "4000000000000004", new DateTime(2026, 1, 31, 9, 0, 0, 0, DateTimeKind.Utc), 0 },
                    { 46, 5, 1000m, new DateTime(2026, 1, 5, 11, 0, 0, 0, DateTimeKind.Utc), "4000000000000005", new DateTime(2026, 1, 5, 11, 0, 0, 0, DateTimeKind.Utc), 0 },
                    { 47, 5, 850m, new DateTime(2026, 1, 9, 11, 0, 0, 0, DateTimeKind.Utc), "4000000000000005", new DateTime(2026, 1, 9, 11, 0, 0, 0, DateTimeKind.Utc), 0 },
                    { 48, 5, 1200m, new DateTime(2026, 1, 13, 11, 0, 0, 0, DateTimeKind.Utc), "4000000000000005", new DateTime(2026, 1, 13, 11, 0, 0, 0, DateTimeKind.Utc), 0 },
                    { 49, 5, 950m, new DateTime(2026, 1, 17, 11, 0, 0, 0, DateTimeKind.Utc), "4000000000000005", new DateTime(2026, 1, 17, 11, 0, 0, 0, DateTimeKind.Utc), 0 },
                    { 50, 5, 1100m, new DateTime(2026, 1, 21, 11, 0, 0, 0, DateTimeKind.Utc), "4000000000000005", new DateTime(2026, 1, 21, 11, 0, 0, 0, DateTimeKind.Utc), 0 },
                    { 51, 5, 750m, new DateTime(2026, 1, 25, 11, 0, 0, 0, DateTimeKind.Utc), "4000000000000005", new DateTime(2026, 1, 25, 11, 0, 0, 0, DateTimeKind.Utc), 0 },
                    { 52, 5, 900m, new DateTime(2026, 1, 28, 11, 0, 0, 0, DateTimeKind.Utc), "4000000000000005", new DateTime(2026, 1, 28, 11, 0, 0, 0, DateTimeKind.Utc), 0 }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "AccountId", "FullName" },
                values: new object[,]
                {
                    { 1, 1, "Juan Pérez" },
                    { 2, 2, "María García" },
                    { 3, 3, "Carlos López" },
                    { 4, 4, "Ana Martínez" },
                    { 5, 5, "Pedro Rodríguez" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Cards_AccountId",
                table: "Cards",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Cards_CardNumber",
                table: "Cards",
                column: "CardNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Operations_AccountId",
                table: "Operations",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_AccountId",
                table: "Users",
                column: "AccountId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Cards");

            migrationBuilder.DropTable(
                name: "Operations");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Accounts");
        }
    }
}

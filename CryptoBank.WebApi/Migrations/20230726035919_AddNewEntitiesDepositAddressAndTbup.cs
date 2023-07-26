using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CryptoBank.WebApi.Migrations
{
    /// <inheritdoc />
    public partial class AddNewEntitiesDepositAddressAndTbup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Tpubs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CurrencyCode = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tpubs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DepositAddresses",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CurrencyCode = table.Column<string>(type: "text", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    TpubId = table.Column<long>(type: "bigint", nullable: false),
                    DerivationIndex = table.Column<int>(type: "integer", nullable: false),
                    CryptoAddress = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DepositAddresses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DepositAddresses_Tpubs_TpubId",
                        column: x => x.TpubId,
                        principalTable: "Tpubs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DepositAddresses_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DepositAddresses_TpubId",
                table: "DepositAddresses",
                column: "TpubId");

            migrationBuilder.CreateIndex(
                name: "IX_DepositAddresses_UserId",
                table: "DepositAddresses",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DepositAddresses");

            migrationBuilder.DropTable(
                name: "Tpubs");
        }
    }
}

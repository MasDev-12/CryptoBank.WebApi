using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CryptoBank.WebApi.Migrations
{
    /// <inheritdoc />
    public partial class RenamePropertiesRefreshToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TokenExpirePeriod",
                table: "RefreshTokens",
                newName: "TokenValidityPeriod");

            migrationBuilder.RenameColumn(
                name: "ExpiryDate",
                table: "RefreshTokens",
                newName: "TokenStoragePeriod");

            migrationBuilder.AlterColumn<long>(
                name: "ReplacedByNextToken",
                table: "RefreshTokens",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TokenValidityPeriod",
                table: "RefreshTokens",
                newName: "TokenExpirePeriod");

            migrationBuilder.RenameColumn(
                name: "TokenStoragePeriod",
                table: "RefreshTokens",
                newName: "ExpiryDate");

            migrationBuilder.AlterColumn<long>(
                name: "ReplacedByNextToken",
                table: "RefreshTokens",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LeafBidAPI.Migrations
{
    /// <inheritdoc />
    public partial class MakeHouseNumberSuffixNullableAndAddCompanyToRegisteredProduct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CompanyId",
                table: "RegisteredProducts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "HouseNumberSuffix",
                table: "Companies",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_RegisteredProducts_CompanyId",
                table: "RegisteredProducts",
                column: "CompanyId");

            migrationBuilder.AddForeignKey(
                name: "FK_RegisteredProducts_Companies_CompanyId",
                table: "RegisteredProducts",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RegisteredProducts_Companies_CompanyId",
                table: "RegisteredProducts");

            migrationBuilder.DropIndex(
                name: "IX_RegisteredProducts_CompanyId",
                table: "RegisteredProducts");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "RegisteredProducts");

            migrationBuilder.AlterColumn<string>(
                name: "HouseNumberSuffix",
                table: "Companies",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }
    }
}

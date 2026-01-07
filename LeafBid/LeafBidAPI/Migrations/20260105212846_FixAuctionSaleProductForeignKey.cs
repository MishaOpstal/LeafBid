using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LeafBidAPI.Migrations
{
    /// <inheritdoc />
    public partial class FixAuctionSaleProductForeignKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AuctionSaleProducts_Products_ProductId",
                table: "AuctionSaleProducts");

            migrationBuilder.RenameColumn(
                name: "ProductId",
                table: "AuctionSaleProducts",
                newName: "RegisteredProductId");

            migrationBuilder.RenameIndex(
                name: "IX_AuctionSaleProducts_ProductId",
                table: "AuctionSaleProducts",
                newName: "IX_AuctionSaleProducts_RegisteredProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_AuctionSaleProducts_RegisteredProducts_RegisteredProductId",
                table: "AuctionSaleProducts",
                column: "RegisteredProductId",
                principalTable: "RegisteredProducts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AuctionSaleProducts_RegisteredProducts_RegisteredProductId",
                table: "AuctionSaleProducts");

            migrationBuilder.RenameColumn(
                name: "RegisteredProductId",
                table: "AuctionSaleProducts",
                newName: "ProductId");

            migrationBuilder.RenameIndex(
                name: "IX_AuctionSaleProducts_RegisteredProductId",
                table: "AuctionSaleProducts",
                newName: "IX_AuctionSaleProducts_ProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_AuctionSaleProducts_Products_ProductId",
                table: "AuctionSaleProducts",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}

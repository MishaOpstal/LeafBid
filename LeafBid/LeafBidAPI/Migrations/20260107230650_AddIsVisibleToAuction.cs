using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LeafBidAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddIsVisibleToAuction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsVisible",
                table: "Auctions",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsVisible",
                table: "Auctions");
        }
    }
}

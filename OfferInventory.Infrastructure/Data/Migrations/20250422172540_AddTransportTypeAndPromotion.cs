using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OfferInventory.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTransportTypeAndPromotion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPromoted",
                table: "TransportOffers",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "TransportType",
                table: "TransportOffers",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPromoted",
                table: "TransportOffers");

            migrationBuilder.DropColumn(
                name: "TransportType",
                table: "TransportOffers");
        }
    }
}

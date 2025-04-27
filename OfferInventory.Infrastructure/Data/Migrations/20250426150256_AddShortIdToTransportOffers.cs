using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OfferInventory.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddShortIdToTransportOffers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ShortId",
                table: "TransportOffers",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShortId",
                table: "TransportOffers");
        }
    }
}

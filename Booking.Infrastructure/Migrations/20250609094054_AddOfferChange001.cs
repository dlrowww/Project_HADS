using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Booking.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOfferChange001 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_OfferChange",
                table: "OfferChange");

            migrationBuilder.RenameTable(
                name: "OfferChange",
                newName: "OfferChanges");

            migrationBuilder.RenameIndex(
                name: "IX_OfferChange_OfferId",
                table: "OfferChanges",
                newName: "IX_OfferChanges_OfferId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OfferChanges",
                table: "OfferChanges",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_OfferChanges",
                table: "OfferChanges");

            migrationBuilder.RenameTable(
                name: "OfferChanges",
                newName: "OfferChange");

            migrationBuilder.RenameIndex(
                name: "IX_OfferChanges_OfferId",
                table: "OfferChange",
                newName: "IX_OfferChange_OfferId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OfferChange",
                table: "OfferChange",
                column: "Id");
        }
    }
}

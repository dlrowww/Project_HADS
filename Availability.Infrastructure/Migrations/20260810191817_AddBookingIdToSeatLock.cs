using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Availability.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBookingIdToSeatLock : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "BookingId",
                table: "SeatLocks",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.Sql("UPDATE SeatLocks SET BookingId = LockId WHERE BookingId IS NULL");

            migrationBuilder.AlterColumn<Guid>(
                name: "BookingId",
                table: "SeatLocks",
                type: "char(36)",
                nullable: false,
                collation: "ascii_general_ci",
                oldClrType: typeof(Guid),
                oldType: "char(36)",
                oldNullable: true,
                oldCollation: "ascii_general_ci");

            migrationBuilder.CreateIndex(
                name: "IX_SeatLocks_BookingId",
                table: "SeatLocks",
                column: "BookingId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SeatLocks_BookingId",
                table: "SeatLocks");

            migrationBuilder.DropColumn(
                name: "BookingId",
                table: "SeatLocks");
        }
    }
}

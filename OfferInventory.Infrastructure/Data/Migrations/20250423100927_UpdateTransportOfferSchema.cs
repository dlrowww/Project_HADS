using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OfferInventory.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTransportOfferSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TransportType",
                table: "TransportOffers",
                newName: "TransferType");

            migrationBuilder.RenameColumn(
                name: "Price",
                table: "TransportOffers",
                newName: "PriceTotal");

            migrationBuilder.RenameColumn(
                name: "MaxSeats",
                table: "TransportOffers",
                newName: "SeatsAvailable");

            migrationBuilder.RenameColumn(
                name: "AvailableSeats",
                table: "TransportOffers",
                newName: "IntermediateStations");

            migrationBuilder.AlterColumn<TimeOnly>(
                name: "DepartureTime",
                table: "TransportOffers",
                type: "time(6)",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)");

            migrationBuilder.AlterColumn<TimeOnly>(
                name: "ArrivalTime",
                table: "TransportOffers",
                type: "time(6)",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)");

            migrationBuilder.AddColumn<DateOnly>(
                name: "ArrivalDate",
                table: "TransportOffers",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.AddColumn<DateOnly>(
                name: "DepartureDate",
                table: "TransportOffers",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.AddColumn<int>(
                name: "DurationHours",
                table: "TransportOffers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DurationMinutes",
                table: "TransportOffers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "FromStation",
                table: "TransportOffers",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "MeansOfTransport",
                table: "TransportOffers",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<decimal>(
                name: "PriceAverage",
                table: "TransportOffers",
                type: "decimal(65,30)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PriceOriginal",
                table: "TransportOffers",
                type: "decimal(65,30)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "TransportOffers",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "ToStation",
                table: "TransportOffers",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ArrivalDate",
                table: "TransportOffers");

            migrationBuilder.DropColumn(
                name: "DepartureDate",
                table: "TransportOffers");

            migrationBuilder.DropColumn(
                name: "DurationHours",
                table: "TransportOffers");

            migrationBuilder.DropColumn(
                name: "DurationMinutes",
                table: "TransportOffers");

            migrationBuilder.DropColumn(
                name: "FromStation",
                table: "TransportOffers");

            migrationBuilder.DropColumn(
                name: "MeansOfTransport",
                table: "TransportOffers");

            migrationBuilder.DropColumn(
                name: "PriceAverage",
                table: "TransportOffers");

            migrationBuilder.DropColumn(
                name: "PriceOriginal",
                table: "TransportOffers");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "TransportOffers");

            migrationBuilder.DropColumn(
                name: "ToStation",
                table: "TransportOffers");

            migrationBuilder.RenameColumn(
                name: "TransferType",
                table: "TransportOffers",
                newName: "TransportType");

            migrationBuilder.RenameColumn(
                name: "SeatsAvailable",
                table: "TransportOffers",
                newName: "MaxSeats");

            migrationBuilder.RenameColumn(
                name: "PriceTotal",
                table: "TransportOffers",
                newName: "Price");

            migrationBuilder.RenameColumn(
                name: "IntermediateStations",
                table: "TransportOffers",
                newName: "AvailableSeats");

            migrationBuilder.AlterColumn<DateTime>(
                name: "DepartureTime",
                table: "TransportOffers",
                type: "datetime(6)",
                nullable: false,
                oldClrType: typeof(TimeOnly),
                oldType: "time(6)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "ArrivalTime",
                table: "TransportOffers",
                type: "datetime(6)",
                nullable: false,
                oldClrType: typeof(TimeOnly),
                oldType: "time(6)");
        }
    }
}

using System;
namespace Search.API.Models;

public class TransportOfferDto
{
    public Guid     Id             { get; set; }
    public DateTime DepartureTime  { get; set; }
    public DateTime ArrivalTime    { get; set; }
    public decimal  Price          { get; set; }
    public string   Currency       { get; set; } = "USD";
    public string   Provider       { get; set; } = "";
    public string   FromCity       { get; set; } = "";
    public string   ToCity         { get; set; } = "";
    public int      MaxSeats       { get; set; } = 50;
    public int      AvailableSeats { get; set; } = 50;
    public string   TransportType  { get; set; } = "bus";   // 你在 OfferInventory 里存的字段名
}

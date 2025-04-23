using System;

namespace Search.API.Models
{
    public class TransportOfferDto
    {
        public Guid      Id             { get; set; }
        public TimeOnly  DepartureTime  { get; set; }
        public TimeOnly  ArrivalTime    { get; set; }
        public decimal   Price          { get; set; }
        public string    Currency       { get; set; } = "USD";
        public string    Provider       { get; set; } = "";
        public string    FromCity       { get; set; } = "";
        public string    ToCity         { get; set; } = "";
        public int       AvailableSeats { get; set; }
        public bool      IsPromoted     { get; set; }
        public string    Transport  { get; set; } = "bus";
    }
}

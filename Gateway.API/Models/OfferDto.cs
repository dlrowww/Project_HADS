using System;


namespace Gateway.API.Models
{
    public class OfferDto
    {
        public string  ShortId        { get; set; } = "";
        //public Guid    Id             { get; set; }
        public string  Provider       { get; set; } = "";
        public string  Status         { get; set; } = "";
        public TimeOnly DepartureTime { get; set; }
        public TimeOnly ArrivalTime   { get; set; }
        public decimal Price          { get; set; }
        public string  Currency       { get; set; } = "";
        public string  FromCity       { get; set; } = "";
        public string  ToCity         { get; set; } = "";
        public int     AvailableSeats { get; set; }
        public bool    IsPromoted     { get; set; }
        public string  Transport      { get; set; } = "";

        public int     DurationHours     { get; set; }
        public int     DurationMinutes   { get; set; }
    }
}

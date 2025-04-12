using OfferInventory.Domain.Enums;
using OfferInventory.Domain.Entities;
using System;


namespace OfferInventory.Domain.Entities
{
    public class TransportOffer
    {
        public Guid Id { get; set; }
        public TransportType Type { get; set; }
        public string CarrierName { get; set; } = string.Empty;
        public string Origin { get; set; } = string.Empty;
        public string Destination { get; set; } = string.Empty;
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }

        // 关联的座位信息
        public List<Seat> Seats { get; set; } = new();
    }
}


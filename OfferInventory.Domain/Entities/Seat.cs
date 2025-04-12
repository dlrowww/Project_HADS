namespace OfferInventory.Domain.Entities
{
    public class Seat
    {
        public Guid Id { get; set; }
        public string SeatNumber { get; set; } = string.Empty;
        public bool IsAvailable { get; set; } = true;

        // 关联 TransportOffer 的 ID
        public Guid TransportOfferId { get; set; }
    }
}


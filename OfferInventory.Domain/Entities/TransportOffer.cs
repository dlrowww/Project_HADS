namespace OfferInventory.Domain.Entities;

public class TransportOffer
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime DepartureTime { get; set; }
    public DateTime ArrivalTime   { get; set; }
    public decimal  Price         { get; set; }
    public string   Currency      { get; set; } = "USD";
    public string   Provider      { get; set; } = "FakeProvider";
    public string   FromCity      { get; set; } = "";
    public string   ToCity        { get; set; } = "";
    public int MaxSeats           { get; set; } = 50;
    public int AvailableSeats     { get; set; } = 50;
}

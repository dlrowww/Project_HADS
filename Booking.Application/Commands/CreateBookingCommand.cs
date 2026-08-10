namespace Booking.Application.Commands
{
    public class CreateBookingCommand
    {
        public Guid OfferId { get; set; }             // 班次 ID
        public int NumberOfSeats { get; set; }        // 订票张数

        public decimal TotalPrice { get; set; }       // 
        public string Currency { get; set; } = "GBP"; // 

        public string FromCity { get; set; } = null!;
        public string ToCity { get; set; } = null!;

        public string CustomerName { get; set; } = null!;
    }
}

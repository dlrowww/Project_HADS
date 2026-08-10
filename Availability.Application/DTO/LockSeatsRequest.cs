namespace Availability.Application.DTO
{
    public class LockSeatsRequest
    {
        public Guid BookingId { get; set; }
        public Guid OfferId { get; set; }
        public Guid UserId { get; set; }
        public int NumberOfSeats { get; set; }
    }
}
using Booking.Domain.Enums;

namespace Booking.Domain.Entities
{
    public class Booking
    {
        public Guid BookingId { get; set; }         // 主键
        public Guid UserId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public Guid OfferId { get; set; }           // 对应班次
        public int NumberOfSeats { get; set; }      // 订票张数
        public Guid? LockId { get; set; }            // SeatLock ID
        public BookingStatus Status { get; set; }   // 当前状态
        public DateTime CreatedAt { get; set; }     // 下单时间
        public DateTime? PaidAt { get; set; }       // 支付时间（可空）
        public decimal TotalPrice { get; set; }
        public string Currency { get; set; } = "GDB";

        // 构造函数
        private Booking() { }

        public Booking(Guid bookingId, Guid userId, string customerName, Guid offerId, int numberOfSeats, Guid? lockId)
        {
            BookingId     = bookingId;
            UserId        = userId;
            CustomerName  = customerName;
            OfferId       = offerId;
            NumberOfSeats = numberOfSeats;
            LockId        = lockId;
            Status        = BookingStatus.Created;
            CreatedAt     = DateTime.UtcNow;
        }

        // 状态变更逻辑
        public void MarkAsPaid()
        {
            Status = BookingStatus.Paid;
            PaidAt = DateTime.UtcNow;
        }

        public void MarkAsFailed()
        {
            Status = BookingStatus.Failed;
        }

        public void Cancel()
        {
            Status = BookingStatus.Cancelled;
        }
    }
}

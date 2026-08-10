using Payment.Domain.Enums;   // ✔ 用已有枚举

namespace Payment.Domain.Entities
{
    public class PaymentRecord
    {
        public Guid PaymentId      { get; set; } = Guid.NewGuid();
        public Guid BookingId      { get; set; }
        public decimal Amount      { get; set; }
        public string Currency     { get; set; } = "CNY";
        public PaymentStatus Status { get; private set; } = PaymentStatus.Pending;
        public string TransactionId { get; private set; } = Guid.NewGuid().ToString();
        public DateTime CreatedAt   { get; } = DateTime.UtcNow;
        public DateTime? PaidAt     { get; private set; }

        public void MarkSuccess()  { Status = PaymentStatus.Success;  PaidAt = DateTime.UtcNow; }
        public void MarkFailed()   { Status = PaymentStatus.Failed;   }
        public void MarkCanceled() { Status = PaymentStatus.Canceled; }
    }
}
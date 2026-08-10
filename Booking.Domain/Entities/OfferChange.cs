using System;

namespace Booking.Domain.Entities
{
    /// <summary>
    /// 模拟 Trip-Operator 侧对 Offer 的变更日志
    /// </summary>
    public class OfferChange
    {
        public Guid    Id         { get; set; } = Guid.NewGuid();
        public Guid    OfferId    { get; set; }
        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
        public string  Field      { get; set; } = null!;   // "price" 或 "availability"
        public string  OldValue   { get; set; } = null!;
        public string  NewValue   { get; set; } = null!;
    }
}


using Availability.Domain.Enums;

namespace Availability.Domain.Entities;
public class SeatLock
{
    public Guid LockId { get; private set; } = Guid.NewGuid();
    public Guid BookingId { get; private set; }
    public Guid OfferId { get; private set; }
    public Guid UserId { get; private set; }
    public int NumberOfSeats { get; private set; }
    public DateTime LockedAt { get; private set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; private set; }
    public SeatLockStatus Status { get; private set; } = SeatLockStatus.Locked;

    // ⚠️ 必须提供一个无参构造函数给 EF Core 用（私有也可以）
    private SeatLock() { }

    // ✅ 业务工厂构造函数 —— EF 不使用它
    public static SeatLock Create(Guid bookingId, Guid offerId, Guid userId, int seats, TimeSpan ttl)
        => new(bookingId, offerId, userId, seats, ttl);

    private SeatLock(Guid bookingId, Guid offerId, Guid userId, int seats, TimeSpan ttl)
    {
        BookingId = bookingId;
        OfferId = offerId;
        UserId = userId;
        NumberOfSeats = seats;
        LockedAt = DateTime.UtcNow;
        ExpiresAt = LockedAt.Add(ttl);
        Status = SeatLockStatus.Locked;
    }

    public void Release() => Status = SeatLockStatus.Released;
    public void Commit() => Status = SeatLockStatus.Committed;
}

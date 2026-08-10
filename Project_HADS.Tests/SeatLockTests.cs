using Availability.Domain.Entities;
using Availability.Domain.Enums;
using Xunit;

namespace Project_HADS.Tests;

public class SeatLockTests
{
    [Fact]
    public void Lock_tracks_booking_user_and_seat_count()
    {
        var bookingId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var offerId = Guid.NewGuid();

        var seatLock = SeatLock.Create(bookingId, offerId, userId, 2, TimeSpan.FromMinutes(15));

        Assert.Equal(bookingId, seatLock.BookingId);
        Assert.Equal(userId, seatLock.UserId);
        Assert.Equal(offerId, seatLock.OfferId);
        Assert.Equal(2, seatLock.NumberOfSeats);
        Assert.Equal(SeatLockStatus.Locked, seatLock.Status);
    }

    [Fact]
    public void Lock_can_be_committed_or_released()
    {
        var committed = SeatLock.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 1, TimeSpan.FromMinutes(15));
        var released = SeatLock.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 1, TimeSpan.FromMinutes(15));

        committed.Commit();
        released.Release();

        Assert.Equal(SeatLockStatus.Committed, committed.Status);
        Assert.Equal(SeatLockStatus.Released, released.Status);
    }
}

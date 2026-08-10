using Booking.Domain.Entities;
using Xunit;

namespace Project_HADS.Tests;

public class BookingTests
{
    [Fact]
    public void Booking_keeps_authenticated_user_and_lock_identity()
    {
        var bookingId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var lockId = Guid.NewGuid();

        var booking = new Booking.Domain.Entities.Booking(
            bookingId, userId, "customer", Guid.NewGuid(), 2, lockId);

        Assert.Equal(bookingId, booking.BookingId);
        Assert.Equal(userId, booking.UserId);
        Assert.Equal(lockId, booking.LockId);
    }
}

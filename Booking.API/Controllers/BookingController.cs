using Booking.Application.Commands;
using Booking.Application.CommandHandler;
using Booking.Domain.Entities;
using Booking.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using Booking.API.Hubs;

namespace Booking.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BookingController : ControllerBase
{
    private readonly BookingDbContext _db;
    private readonly CreateBookingHandler _handler;
    private readonly IHubContext<BookingStatusHub> _hub;

    public BookingController(BookingDbContext db, CreateBookingHandler handler, IHubContext<BookingStatusHub> hub)
    {
        _db = db;
        _handler = handler;
        _hub = hub;
    }

    /// <summary>
    /// 创建预订请求，触发 Saga：锁座 → 支付
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateBooking([FromBody] CreateBookingCommand command)
    {
        try
        {
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdValue, out var userId)) return Unauthorized();
            command.UserId = userId;
            var bookingId = await _handler.HandleAsync(command);
            await _hub.Clients.Group($"offer:{command.OfferId}").SendAsync("OfferPurchased", new
            {
                BookingId = bookingId,
                command.OfferId,
                command.NumberOfSeats,
                Status = "Completed",
                Time = DateTime.UtcNow
            });
            return Ok(new { BookingId = bookingId });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// 查询某个 Booking 的状态（用于调试）
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetBookingStatus(Guid id)
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdValue, out var userId)) return Unauthorized();
        var booking = await _db.Bookings.AsNoTracking()
            .SingleOrDefaultAsync(b => b.BookingId == id && b.UserId == userId);

        if (booking == null)
            return NotFound();

        return Ok(new
        {
            booking.BookingId,
            booking.UserId,
            booking.Status,
            booking.CreatedAt,
            booking.PaidAt,
            booking.LockId,
            booking.OfferId,
            booking.NumberOfSeats
        });
    }
}

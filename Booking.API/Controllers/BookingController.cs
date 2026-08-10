using Booking.Application.Commands;
using Booking.Application.CommandHandler;
using Booking.Domain.Entities;
using Booking.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace Booking.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BookingController : ControllerBase
{
    private readonly BookingDbContext _db;
    private readonly CreateBookingHandler _handler;

    public BookingController(BookingDbContext db, CreateBookingHandler handler)
    {
        _db = db;
        _handler = handler;
    }

    /// <summary>
    /// 创建预订请求，触发 Saga：锁座 → 支付
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateBooking([FromBody] CreateBookingCommand command)
    {
        try
        {
            // 直接调用 handler，替代 MediatR
            var bookingId = await _handler.HandleAsync(command);
            return Ok(new { BookingId = bookingId });
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
        var booking = await _db.Bookings.FindAsync(id);

        if (booking == null)
            return NotFound();

        return Ok(new
        {
            booking.BookingId,
            booking.Status,
            booking.CreatedAt,
            booking.PaidAt,
            booking.LockId,
            booking.OfferId,
            booking.NumberOfSeats
        });
    }
}

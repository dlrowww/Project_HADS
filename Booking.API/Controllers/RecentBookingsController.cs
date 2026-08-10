using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Booking.Infrastructure;

namespace Booking.API.Controllers
{
    [ApiController]
    [Route("api/offers/{offerId:guid}/bookings")]
    public class RecentBookingsController : ControllerBase
    {
        private readonly BookingDbContext _db;
        public RecentBookingsController(BookingDbContext db) => _db = db;

        [HttpGet("recent")]
        public async Task<IActionResult> GetRecent(Guid offerId)
        {
            var recent = await _db.Bookings
                .Where(b => b.OfferId == offerId)
                .OrderByDescending(b => b.CreatedAt)
                .Take(10)
                .Select(b => new {
                    b.BookingId,                // ← 新增
                    b.CustomerName,
                    b.NumberOfSeats,
                    Timestamp = b.CreatedAt.ToString("HH:mm:ss")
                })
                .ToListAsync();

            return Ok(recent);
        }
    }
}

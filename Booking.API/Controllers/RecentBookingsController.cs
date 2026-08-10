using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Booking.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Booking.API.Controllers
{
    [ApiController]
    [Route("api/offers/{offerId:guid}/bookings")]
    [Authorize]
    public class RecentBookingsController : ControllerBase
    {
        private readonly BookingDbContext _db;
        public RecentBookingsController(BookingDbContext db) => _db = db;

        [HttpGet("recent")]
        public async Task<IActionResult> GetRecent(Guid offerId)
        {
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdValue, out var userId)) return Unauthorized();
            var recent = await _db.Bookings
                .Where(b => b.OfferId == offerId && b.UserId == userId)
                .OrderByDescending(b => b.CreatedAt)
                .Take(10)
                .Select(b => new {
                    b.BookingId,
                    b.NumberOfSeats,
                    Timestamp = b.CreatedAt.ToString("HH:mm:ss")
                })
                .ToListAsync();

            return Ok(recent);
        }
    }
}

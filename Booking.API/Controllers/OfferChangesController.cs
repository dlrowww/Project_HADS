using System;
using System.Linq;
using System.Threading.Tasks;
using Booking.Domain.Entities;
using Booking.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Booking.API.Controllers
{
    [ApiController]
    [Route("api/offers/{offerId:guid}/changes")]
    public class OfferChangesController : ControllerBase
    {
        private readonly BookingDbContext _db;
        private readonly ILogger<OfferChangesController> _logger;

        public OfferChangesController(BookingDbContext db, ILogger<OfferChangesController> logger)
        {
            _db = db;
            _logger = logger;
        }

        [HttpGet("recent")]
        public async Task<IActionResult> GetRecent(Guid offerId)
        {
            _logger.LogInformation("[Booking.API] Received GetRecent request for OfferId: {OfferId}", offerId);
            Console.WriteLine($"[Booking.API] Start querying changes for OfferId: {offerId}");

            try
            {
                var list = await _db.OfferChanges
                    .Where(c => c.OfferId == offerId)
                    .OrderByDescending(c => c.ChangedAt)
                    .Take(10)
                    .Select(c => new
                    {
                        Id = c.Id,
                        OfferId = c.OfferId,    // 新增 OfferId 字段
                        Field = c.Field,
                        OldValue = c.OldValue,
                        NewValue = c.NewValue,
                        ChangedAt = c.ChangedAt
                    })
                    .ToListAsync();

                _logger.LogInformation("[Booking.API] Retrieved {Count} records", list.Count);
                Console.WriteLine($"[Booking.API] Retrieved {list.Count} change records");

                return Ok(list);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Booking.API] Error querying changes for OfferId: {OfferId}", offerId);
                Console.WriteLine($"[Booking.API] Exception: {ex.Message}");
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }
    }
}
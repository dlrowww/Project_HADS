using Booking.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace Booking.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PreferenceStatsController : ControllerBase
{
    private readonly BookingDbContext _db;

    public PreferenceStatsController(BookingDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// 返回前10条最常被预订的路线
    /// </summary>
    [HttpGet("top")]
    public async Task<IActionResult> GetTopStats()
    {
        var topStats = await _db.DeparturePreferenceStats
            .OrderByDescending(s => s.TotalCount)
            .Take(10)
            .ToListAsync();

        return Ok(topStats);
    }
}

using Availability.Application.DTO;
using Availability.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Availability.Infrastructure;
using Availability.Domain.Entities;

namespace Availability.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AvailabilityController : ControllerBase
{
    private readonly AvailabilityDbContext _db;

    public AvailabilityController(AvailabilityDbContext db)
    {
        _db = db;
    }

    [HttpPost("lock")]
    public async Task<IActionResult> LockSeats([FromBody] LockSeatsRequest req)
    {
        var seatLock = SeatLock.Create(
            req.OfferId,
            req.UserId,
            req.NumberOfSeats,
            TimeSpan.FromMinutes(15)
        );

        _db.SeatLocks.Add(seatLock);
        await _db.SaveChangesAsync();

        return Ok(new { LockId = seatLock.LockId });
    }
    
    [HttpPost("release")]
    public async Task<IActionResult> ReleaseSeats([FromBody] Guid lockId)
    {
        var lockEntity = await _db.SeatLocks.FindAsync(lockId);
        if (lockEntity is null) return NotFound();

        lockEntity.Release(); // 修改状态为 Released
        await _db.SaveChangesAsync();

        return Ok();
    }
}

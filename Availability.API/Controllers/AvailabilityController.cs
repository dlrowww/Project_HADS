using Availability.Application.DTO;
using Availability.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Availability.Infrastructure;
using Availability.Domain.Entities;
using Microsoft.EntityFrameworkCore;

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
        if (req.BookingId == Guid.Empty || req.OfferId == Guid.Empty || req.UserId == Guid.Empty || req.NumberOfSeats <= 0)
            return BadRequest(new { error = "BookingId, OfferId, UserId and a positive NumberOfSeats are required." });

        var existing = await _db.SeatLocks.AsNoTracking()
            .SingleOrDefaultAsync(x => x.BookingId == req.BookingId);
        if (existing is not null)
            return existing.Status == SeatLockStatus.Locked || existing.Status == SeatLockStatus.Committed
                ? Ok(new { existing.LockId })
                : Conflict(new { error = "This booking's seat lock has already been released." });

        var strategy = _db.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync<IActionResult>(async () =>
        {
            await using var transaction = await _db.Database.BeginTransactionAsync();
            var affected = await _db.Database.ExecuteSqlInterpolatedAsync($@"
                UPDATE offer_inventory.TransportOffers
                SET SeatsAvailable = SeatsAvailable - {req.NumberOfSeats}
                WHERE Id = {req.OfferId} AND SeatsAvailable >= {req.NumberOfSeats}");

            if (affected != 1)
            {
                await transaction.RollbackAsync();
                return Conflict(new { error = "Offer does not exist or has insufficient seats." });
            }

            var seatLock = SeatLock.Create(
                req.BookingId,
                req.OfferId,
                req.UserId,
                req.NumberOfSeats,
                TimeSpan.FromMinutes(15)
            );

            _db.SeatLocks.Add(seatLock);
            await _db.SaveChangesAsync();
            await transaction.CommitAsync();

            return Ok(new { LockId = seatLock.LockId });
        });
    }
    
    [HttpPost("release/{lockId:guid}")]
    public async Task<IActionResult> ReleaseSeats(Guid lockId)
    {
        var lockEntity = await _db.SeatLocks.FindAsync(lockId);
        if (lockEntity is null) return NotFound();

        var strategy = _db.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync<IActionResult>(async () =>
        {
            await using var transaction = await _db.Database.BeginTransactionAsync();
            var released = await _db.Database.ExecuteSqlInterpolatedAsync($@"
                UPDATE SeatLocks SET Status = 'Released'
                WHERE LockId = {lockId} AND Status = 'Locked'");
            if (released == 0)
            {
                await transaction.RollbackAsync();
                return lockEntity.Status == SeatLockStatus.Released
                    ? Ok()
                    : Conflict(new { error = "A committed lock cannot be released." });
            }
            await _db.Database.ExecuteSqlInterpolatedAsync($@"
                UPDATE offer_inventory.TransportOffers
                SET SeatsAvailable = SeatsAvailable + {lockEntity.NumberOfSeats}
                WHERE Id = {lockEntity.OfferId}");
            await transaction.CommitAsync();

            return Ok();
        });
    }

    [HttpPost("commit/{lockId:guid}")]
    public async Task<IActionResult> CommitSeats(Guid lockId)
    {
        var lockEntity = await _db.SeatLocks.FindAsync(lockId);
        if (lockEntity is null) return NotFound();
        if (lockEntity.Status == SeatLockStatus.Committed) return Ok();
        var committed = await _db.Database.ExecuteSqlInterpolatedAsync($@"
            UPDATE SeatLocks SET Status = 'Committed'
            WHERE LockId = {lockId} AND Status = 'Locked'");
        if (committed == 0)
        {
            return Conflict(new { error = "A released lock cannot be committed." });
        }
        return Ok();
    }
}

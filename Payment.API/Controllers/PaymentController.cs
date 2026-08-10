using Microsoft.AspNetCore.Mvc;
using Payment.Application.DTO;
using Payment.Domain.Entities;
using Payment.Domain.Enums;
using System;
using Microsoft.EntityFrameworkCore;
using Payment.Infrastructure;

namespace Payment.API.Controllers;

[ApiController]
[Route("api/payments")]
public class PaymentsController : ControllerBase
{
    private readonly PaymentDbContext _db;
    private readonly IWebHostEnvironment _environment;
    private readonly IConfiguration _configuration;

    public PaymentsController(PaymentDbContext db, IWebHostEnvironment environment, IConfiguration configuration)
    {
        _db = db;
        _environment = environment;
        _configuration = configuration;
    }

    [HttpPost("process")]
    public async Task<IActionResult> Process([FromBody] PaymentRequest req)
    {
        if (req.Amount <= 0) return BadRequest("Invalid amount");

        var record = await _db.Payments.SingleOrDefaultAsync(p => p.BookingId == req.BookingId);
        if (record is not null)
            return Ok(new { success = record.Status == PaymentStatus.Success, record.TransactionId });

        record = new PaymentRecord
        {
            BookingId = req.BookingId,
            Amount    = req.Amount,
            Currency  = req.Currency
        };
        var failureRate = Math.Clamp(_configuration.GetValue<double>("PaymentSimulation:FailureRate"), 0, 1);
        bool success = _environment.IsDevelopment() && req.SimulateSuccess.HasValue
            ? req.SimulateSuccess.Value
            : Random.Shared.NextDouble() >= failureRate;
        if (success) record.MarkSuccess();
        else          record.MarkFailed();

        _db.Payments.Add(record);
        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            _db.ChangeTracker.Clear();
            var existing = await _db.Payments.AsNoTracking()
                .SingleAsync(p => p.BookingId == req.BookingId);
            return Ok(new
            {
                success = existing.Status == PaymentStatus.Success,
                existing.TransactionId
            });
        }

        return Ok(new { success, transactionId = record.TransactionId });
    }

    [HttpGet("booking/{bookingId:guid}")]
    public async Task<IActionResult> GetByBooking(Guid bookingId)
    {
        var record = await _db.Payments.AsNoTracking()
            .SingleOrDefaultAsync(p => p.BookingId == bookingId);
        return record is null
            ? NotFound()
            : Ok(new { record.BookingId, Status = record.Status.ToString(), record.TransactionId });
    }

    // Saga 补偿接口
    [HttpPost("cancel")]
    public async Task<IActionResult> Cancel([FromBody] PaymentRequest req)
    {
        var record = await _db.Payments.SingleOrDefaultAsync(p => p.BookingId == req.BookingId);
        if (record is null)
            return NotFound("No payment to cancel");

        record.MarkCanceled();
        await _db.SaveChangesAsync();
        return Ok();
    }
}

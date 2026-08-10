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
    public PaymentsController(PaymentDbContext db) => _db = db;

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
        bool success = true;
        if (success) record.MarkSuccess();
        else          record.MarkFailed();

        _db.Payments.Add(record);
        await _db.SaveChangesAsync();

        return Ok(new { success, transactionId = record.TransactionId });
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

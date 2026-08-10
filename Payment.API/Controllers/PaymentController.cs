using Microsoft.AspNetCore.Mvc;
using Payment.Application.DTO;
using Payment.Domain.Entities;
using Payment.Domain.Enums;
using System;
using System.Collections.Concurrent;

namespace Payment.API.Controllers;

[ApiController]
[Route("api/payments")]
public class PaymentsController : ControllerBase
{
    // 临时存储，演示用；正式应换 EF Core DbContext
    private static readonly ConcurrentDictionary<Guid, PaymentRecord> _store = new();

    private readonly Random _rng = new();

    [HttpPost("process")]
    public IActionResult Process([FromBody] PaymentRequest req)
    {
        if (req.Amount <= 0) return BadRequest("Invalid amount");

        var record = new PaymentRecord
        {
            BookingId = req.BookingId,
            Amount    = req.Amount,
            Currency  = req.Currency
        };
        bool success =true;           // 50% 成功率
        if (success) record.MarkSuccess();
        else          record.MarkFailed();

        _store[record.BookingId] = record;

        return Ok(new { success, transactionId = record.TransactionId });
    }

    // Saga 补偿接口
    [HttpPost("cancel")]
    public IActionResult Cancel([FromBody] PaymentRequest req)
    {
        if (!_store.TryGetValue(req.BookingId, out var record))
            return NotFound("No payment to cancel");

        record.MarkCanceled();
        return Ok();
    }
}

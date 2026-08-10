using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Booking.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Booking.Application.Sagas
{
    public class BookingSagaCoordinator
    {
        private readonly IHttpClientFactory                  _httpFactory;
        private readonly IDbContextFactory<BookingDbContext> _dbFactory;
        private readonly ILogger<BookingSagaCoordinator>     _logger;

        public BookingSagaCoordinator(
            IHttpClientFactory                  httpFactory,
            IDbContextFactory<BookingDbContext> dbFactory,
            ILogger<BookingSagaCoordinator>     logger)
        {
            _httpFactory = httpFactory;
            _dbFactory   = dbFactory;
            _logger      = logger;
        }

        public async Task StartSagaAsync(Guid bookingId)
        {
            _logger.LogInformation("=== Saga START for Booking {BookingId} ===", bookingId);

            await using var db = _dbFactory.CreateDbContext();
            var booking = await db.Bookings.FindAsync(bookingId);
            if (booking == null)
            {
                _logger.LogWarning("Booking {Id} not found, abort saga.", bookingId);
                return;
            }

            _logger.LogInformation("Loaded Booking {Id}: Status={Status}", bookingId, booking.Status);

            /* ---------- 调 Payment.API ---------- */
            var client      = _httpFactory.CreateClient("payment-api");
            var payloadJson = JsonSerializer.Serialize(new
            {
                booking.BookingId,
                Amount   = booking.TotalPrice,
                booking.Currency
            });

            _logger.LogInformation("Calling Payment API for Booking {Id}", bookingId);

            HttpResponseMessage resp;
            try
            {
                resp = await client.PostAsync(
                    "/api/payments/process",
                    new StringContent(payloadJson, Encoding.UTF8, "application/json"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Payment API unreachable for Booking {Id}", bookingId);
                booking.MarkAsFailed();
                db.Entry(booking).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return;
            }

            _logger.LogInformation("Payment API responded {StatusCode} for Booking {Id}", resp.StatusCode, bookingId);

            var body   = await resp.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<PaymentResult>(body,
                             new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                         ?? new PaymentResult { Success = false };

            if (result.Success)
            {
                booking.MarkAsPaid();
                _logger.LogInformation("Payment 成功 for Booking {Id}, TransactionId={Tx}",
                                       bookingId, result.TransactionId);
            }
            else
            {
                booking.MarkAsFailed();
                _logger.LogWarning("Payment FAILED for Booking {Id}, TransactionId={Tx}",
                                   bookingId, result.TransactionId);
            }

            /* ---- 关键：确保 EF 一定执行 UPDATE ---- */
            Console.WriteLine("🌟 About to call SaveChangesAsync()");
           
            try
            {
                db.Entry(booking).State = EntityState.Modified;

                var rows = await db.SaveChangesAsync();
                Console.WriteLine($"[Saga] SaveChanges affected rows = {rows}");
                _logger.LogInformation("=== Saga 结束 for Booking {BookingId} – Final={Status} ===", bookingId, booking.Status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ SaveChangesAsync failed for Booking {Id}", bookingId);
            }
        }

        /* ----------- internal DTO ----------- */
        private class PaymentResult
        {
            public bool   Success        { get; set; }
            public string? TransactionId { get; set; }
        }
    }
}

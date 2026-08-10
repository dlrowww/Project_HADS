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

        public async Task StartSagaAsync(Guid bookingId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("=== Saga START for Booking {BookingId} ===", bookingId);

            await using var db = _dbFactory.CreateDbContext();
            var booking = await db.Bookings.FindAsync(new object[] { bookingId }, cancellationToken);
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
                // Payment may have committed before its response was lost.
                // Keep Created + Locked until reconciliation resolves the authoritative state.
                _logger.LogError(ex,
                    "Payment result is unknown for Booking {Id}; keeping Created state for reconciliation",
                    bookingId);
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
                await SaveBookingStateAsync(db, booking, cancellationToken);
                await CommitLockAsync(booking.LockId, cancellationToken);
                _logger.LogInformation("Payment 成功 for Booking {Id}, TransactionId={Tx}",
                                       bookingId, result.TransactionId);
            }
            else
            {
                booking.MarkAsFailed();
                await SaveBookingStateAsync(db, booking, cancellationToken);
                await ReleaseLockAsync(booking.LockId, cancellationToken);
                _logger.LogWarning("Payment FAILED for Booking {Id}, TransactionId={Tx}",
                                   bookingId, result.TransactionId);
            }

            _logger.LogInformation("=== Saga END for Booking {BookingId} – Final={Status} ===", bookingId, booking.Status);
        }

        private static async Task SaveBookingStateAsync(
            BookingDbContext db,
            Booking.Domain.Entities.Booking booking,
            CancellationToken ct)
        {
            db.Entry(booking).State = EntityState.Modified;
            await db.SaveChangesAsync(ct);
        }

        private async Task CommitLockAsync(Guid? lockId, CancellationToken ct)
        {
            if (lockId is null) return;
            await SendAvailabilityCommandWithRetryAsync($"commit/{lockId}", ct);
        }

        private async Task ReleaseLockAsync(Guid? lockId, CancellationToken ct)
        {
            if (lockId is null) return;
            await SendAvailabilityCommandWithRetryAsync($"release/{lockId}", ct);
        }

        private async Task SendAvailabilityCommandWithRetryAsync(string command, CancellationToken ct)
        {
            for (var attempt = 1; attempt <= 3; attempt++)
            {
                try
                {
                    var response = await _httpFactory.CreateClient("availability-api")
                        .PostAsync($"/api/availability/{command}", null, ct);
                    if (response.IsSuccessStatusCode) return;
                    _logger.LogWarning("Availability command {Command} failed with {StatusCode} (attempt {Attempt})",
                        command, response.StatusCode, attempt);
                }
                catch (Exception ex) when (attempt < 3 && !ct.IsCancellationRequested)
                {
                    _logger.LogWarning(ex, "Availability command {Command} failed (attempt {Attempt})", command, attempt);
                }
                if (attempt < 3) await Task.Delay(TimeSpan.FromMilliseconds(200 * attempt), ct);
            }
            _logger.LogError("Availability command {Command} remains unsynchronized; reconciliation will retry it", command);
        }

        /* ----------- internal DTO ----------- */
        private class PaymentResult
        {
            public bool   Success        { get; set; }
            public string? TransactionId { get; set; }
        }
    }
}

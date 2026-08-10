using Booking.Domain.Enums;
using Booking.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Json;

namespace Booking.API.HostedServices;

public sealed class BookingReconciliationService : BackgroundService
{
    private readonly IDbContextFactory<BookingDbContext> _dbFactory;
    private readonly IHttpClientFactory _httpFactory;
    private readonly ILogger<BookingReconciliationService> _logger;

    public BookingReconciliationService(
        IDbContextFactory<BookingDbContext> dbFactory,
        IHttpClientFactory httpFactory,
        ILogger<BookingReconciliationService> logger)
    {
        _dbFactory = dbFactory;
        _httpFactory = httpFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ReconcileAsync(stoppingToken);
            }
            catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogError(ex, "Booking/Availability reconciliation pass failed");
            }
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
    }

    private async Task ReconcileAsync(CancellationToken ct)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        var bookings = await db.Bookings
            .Where(b => b.LockId != null &&
                (b.Status == BookingStatus.Created || b.Status == BookingStatus.Paid ||
                 b.Status == BookingStatus.Failed || b.Status == BookingStatus.Cancelled))
            .OrderByDescending(b => b.CreatedAt)
            .Take(200)
            .ToListAsync(ct);

        var availability = _httpFactory.CreateClient("availability-api");
        var payment = _httpFactory.CreateClient("payment-api");
        foreach (var booking in bookings)
        {
            if (booking.Status == BookingStatus.Created)
            {
                var paymentResponse = await payment.GetAsync($"/api/payments/booking/{booking.BookingId}", ct);
                if (!paymentResponse.IsSuccessStatusCode) continue;
                var paymentState = await paymentResponse.Content.ReadFromJsonAsync<PaymentState>(cancellationToken: ct);
                if (paymentState?.Status == "Success") booking.MarkAsPaid();
                else if (paymentState?.Status is "Failed" or "Canceled") booking.MarkAsFailed();
                else continue;
                await db.SaveChangesAsync(ct);
            }

            var operation = booking.Status == BookingStatus.Paid ? "commit" : "release";
            var response = await availability.PostAsync($"/api/availability/{operation}/{booking.LockId}", null, ct);
            if (!response.IsSuccessStatusCode)
                _logger.LogWarning("Reconciliation {Operation} failed for lock {LockId}: {StatusCode}",
                    operation, booking.LockId, response.StatusCode);
        }
    }

    private sealed record PaymentState(string Status);
}

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
    private readonly TimeSpan _paymentResolutionTimeout;

    public BookingReconciliationService(
        IDbContextFactory<BookingDbContext> dbFactory,
        IHttpClientFactory httpFactory,
        IConfiguration configuration,
        ILogger<BookingReconciliationService> logger)
    {
        _dbFactory = dbFactory;
        _httpFactory = httpFactory;
        _logger = logger;
        _paymentResolutionTimeout = TimeSpan.FromMinutes(
            Math.Max(1, configuration.GetValue<int?>("Saga:PaymentResolutionTimeoutMinutes") ?? 10));
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
            try
            {
                if (booking.Status is BookingStatus.Created or BookingStatus.Failed)
                {
                    var paymentResponse = await payment.GetAsync($"/api/payments/booking/{booking.BookingId}", ct);
                    if (paymentResponse.IsSuccessStatusCode)
                    {
                        var paymentState = await paymentResponse.Content
                            .ReadFromJsonAsync<PaymentState>(cancellationToken: ct);
                        if (paymentState?.Status == "Success") booking.MarkAsPaid();
                        else if (paymentState?.Status is "Failed" or "Canceled") booking.MarkAsFailed();
                        else continue;
                        await db.SaveChangesAsync(ct);
                    }
                    else if (paymentResponse.StatusCode == System.Net.HttpStatusCode.NotFound &&
                             DateTime.UtcNow - booking.CreatedAt >= _paymentResolutionTimeout)
                    {
                        booking.MarkAsFailed();
                        await db.SaveChangesAsync(ct);
                    }
                    else
                    {
                        continue;
                    }
                }

                var operation = booking.Status == BookingStatus.Paid ? "commit" : "release";
                var response = await availability.PostAsync(
                    $"/api/availability/{operation}/{booking.LockId}", null, ct);
                if (!response.IsSuccessStatusCode)
                    _logger.LogWarning("Reconciliation {Operation} failed for lock {LockId}: {StatusCode}",
                        operation, booking.LockId, response.StatusCode);
            }
            catch (Exception ex) when (!ct.IsCancellationRequested)
            {
                _logger.LogError(ex, "Could not reconcile Booking {BookingId}; continuing with remaining records",
                    booking.BookingId);
            }
        }
    }

    private sealed record PaymentState(string Status);
}

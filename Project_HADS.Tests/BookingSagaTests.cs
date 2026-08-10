using Booking.Application.Sagas;
using Booking.Domain.Enums;
using Booking.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Project_HADS.Tests;

public class BookingSagaTests
{
    [Fact]
    public async Task Unknown_payment_result_keeps_booking_created_and_does_not_release_seats()
    {
        var options = new DbContextOptionsBuilder<BookingDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var bookingId = Guid.NewGuid();
        await using (var setup = new BookingDbContext(options))
        {
            setup.Bookings.Add(new Booking.Domain.Entities.Booking(
                bookingId, Guid.NewGuid(), "customer", Guid.NewGuid(), 2, Guid.NewGuid())
            {
                TotalPrice = 40,
                Currency = "GBP"
            });
            await setup.SaveChangesAsync();
        }

        var availabilityCalls = 0;
        var clients = new TestHttpClientFactory(
            new HttpClient(new ThrowingHandler()) { BaseAddress = new Uri("http://payment") },
            new HttpClient(new RecordingHandler(() => availabilityCalls++)) { BaseAddress = new Uri("http://availability") });
        var saga = new BookingSagaCoordinator(
            clients,
            new TestDbContextFactory(options),
            NullLogger<BookingSagaCoordinator>.Instance);

        await saga.StartSagaAsync(bookingId);

        await using var verify = new BookingDbContext(options);
        Assert.Equal(BookingStatus.Created, (await verify.Bookings.SingleAsync()).Status);
        Assert.Equal(0, availabilityCalls);
    }

    private sealed class TestDbContextFactory(DbContextOptions<BookingDbContext> options)
        : IDbContextFactory<BookingDbContext>
    {
        public BookingDbContext CreateDbContext() => new(options);
    }

    private sealed class TestHttpClientFactory(HttpClient payment, HttpClient availability)
        : IHttpClientFactory
    {
        public HttpClient CreateClient(string name) => name == "payment-api" ? payment : availability;
    }

    private sealed class ThrowingHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct) =>
            throw new HttpRequestException("response lost");
    }

    private sealed class RecordingHandler(Action record) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            record();
            return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK));
        }
    }
}

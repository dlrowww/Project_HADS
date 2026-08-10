using Booking.Application.Commands;
using Booking.Domain.Entities;
using Booking.Domain.Enums;
using Booking.Infrastructure;
using Booking.Application.Sagas;
using BookingEntity = Booking.Domain.Entities.Booking;
using Microsoft.EntityFrameworkCore;
using OfferInventory.Infrastructure.Data;
using System.Net.Http.Json;
using System;
using System.Threading;
using System.Threading.Tasks;
 // BookingDbContext

namespace Booking.Application.CommandHandler
{
    /// <summary>
    /// 负责把前端传来的 CreateBookingCommand 落库（Booking + 偏好统计表），
    /// 并在写入完成后触发支付 Saga
    /// </summary>
    public class CreateBookingHandler
    {
        private readonly BookingDbContext         _db;
        private readonly BookingSagaCoordinator   _sagaCoordinator;
        private readonly AppDbContext _offers;
        private readonly IHttpClientFactory _httpFactory;

        public CreateBookingHandler(
            BookingDbContext db,
            BookingSagaCoordinator sagaCoordinator,
            AppDbContext offers,
            IHttpClientFactory httpFactory)
        {
            _db              = db;
            _sagaCoordinator = sagaCoordinator;
            _offers = offers;
            _httpFactory = httpFactory;
        }

        public async Task<Guid> HandleAsync(CreateBookingCommand cmd, CancellationToken ct = default)
        {
            if (cmd.UserId == Guid.Empty) throw new UnauthorizedAccessException("Authenticated user id is required.");
            if (cmd.NumberOfSeats <= 0) throw new ArgumentOutOfRangeException(nameof(cmd.NumberOfSeats));

            var offer = await _offers.TransportOffers.AsNoTracking()
                .SingleOrDefaultAsync(o => o.Id == cmd.OfferId, ct)
                ?? throw new KeyNotFoundException("Offer not found.");
            var bookingId = Guid.NewGuid();
            var lockResponse = await _httpFactory.CreateClient("availability-api").PostAsJsonAsync(
                "/api/availability/lock",
                new { BookingId = bookingId, cmd.OfferId, cmd.UserId, cmd.NumberOfSeats }, ct);
            if (!lockResponse.IsSuccessStatusCode)
                throw new InvalidOperationException(await lockResponse.Content.ReadAsStringAsync(ct));
            var lockResult = await lockResponse.Content.ReadFromJsonAsync<LockResult>(cancellationToken: ct)
                ?? throw new InvalidOperationException("Availability returned an invalid response.");

            var booking = new BookingEntity(
                bookingId,
                cmd.UserId,
                customerName : cmd.CustomerName,
                offerId      : cmd.OfferId,
                numberOfSeats: cmd.NumberOfSeats,
                lockId       : lockResult.LockId
            )
            {
                TotalPrice = offer.PriceTotal * cmd.NumberOfSeats,
                Currency   = offer.Currency
            };

            _db.Bookings.Add(booking);
            try
            {
                await _db.SaveChangesAsync(ct);
            }
            catch
            {
                await _httpFactory.CreateClient("availability-api")
                    .PostAsync($"/api/availability/release/{lockResult.LockId}", null, ct);
                throw;
            }

            // 维护 DeparturePreferenceStats 表
            var stat = await _db.DeparturePreferenceStats
                                .FirstOrDefaultAsync(s => s.FromCity == cmd.FromCity
                                                       && s.ToCity   == cmd.ToCity, ct);

            if (stat == null)
            {
                stat = new DeparturePreferenceStat
                {
                    Id            = Guid.NewGuid(),
                    FromCity      = cmd.FromCity,
                    ToCity        = cmd.ToCity,
                    TotalCount    = cmd.NumberOfSeats,
                    LastUpdatedAt = DateTime.UtcNow
                };
                _db.DeparturePreferenceStats.Add(stat);
            }
            else
            {
                stat.TotalCount    += cmd.NumberOfSeats;
                stat.LastUpdatedAt  = DateTime.UtcNow;
            }

            await _db.SaveChangesAsync(ct);  // ← 偏好统计已写入 :contentReference[oaicite:1]{index=1}

            // Fire-and-forget 触发支付 Saga
            //     不 await，后台自动执行扣款或补偿动作
            await _sagaCoordinator.StartSagaAsync(booking.BookingId, ct);

            return booking.BookingId;        // 回传给 Controller → Gateway → 前端
        }

        private sealed record LockResult(Guid LockId);
    }
}

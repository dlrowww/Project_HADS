using Booking.Application.Commands;
using Booking.Domain.Entities;
using Booking.Domain.Enums;
using Booking.Infrastructure;
using Booking.Application.Sagas;
using BookingEntity = Booking.Domain.Entities.Booking;
using Microsoft.EntityFrameworkCore;
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

        public CreateBookingHandler(
            BookingDbContext db,
            BookingSagaCoordinator sagaCoordinator)
        {
            _db              = db;
            _sagaCoordinator = sagaCoordinator;
        }

        public async Task<Guid> HandleAsync(CreateBookingCommand cmd, CancellationToken ct = default)
        {
            // 写入 Booking 表
            var booking = new BookingEntity(
                customerName : cmd.CustomerName,
                offerId      : cmd.OfferId,
                numberOfSeats: cmd.NumberOfSeats,
                lockId       : null
            )
            {
                TotalPrice = cmd.TotalPrice,
                Currency   = cmd.Currency
            };

            _db.Bookings.Add(booking);
            await _db.SaveChangesAsync(ct);  // ← 订票已写入 :contentReference[oaicite:0]{index=0}

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
            _ = _sagaCoordinator.StartSagaAsync(booking.BookingId);

            return booking.BookingId;        // 回传给 Controller → Gateway → 前端
        }
    }
}

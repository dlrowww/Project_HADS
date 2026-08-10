using Microsoft.Extensions.Hosting;           // 为 BackgroundService
using System.Threading;                       // 为 CancellationToken
using System.Threading.Tasks;                 // 为 Task 等异步支持
using Microsoft.EntityFrameworkCore;          // EFCore 查询
using Availability.Infrastructure;            // AvailabilityDbContext
using Availability.Domain.Enums;              //  SeatLockStatus
using Microsoft.Extensions.DependencyInjection;

    // SeatLock 实体
public class SeatLockExpirationService : BackgroundService
{
    private readonly IServiceProvider _provider;

    public SeatLockExpirationService(IServiceProvider provider)
    {
        _provider = provider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _provider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AvailabilityDbContext>();

            var expiredIds = await db.SeatLocks
                .Where(s => s.Status == SeatLockStatus.Locked && s.ExpiresAt < DateTime.UtcNow)
                .Select(s => s.LockId)
                .ToListAsync(stoppingToken);

            foreach (var lockId in expiredIds)
            {
                await using var transaction = await db.Database.BeginTransactionAsync(stoppingToken);
                var seatLock = await db.SeatLocks.SingleOrDefaultAsync(
                    s => s.LockId == lockId && s.Status == SeatLockStatus.Locked,
                    stoppingToken);
                if (seatLock is not null)
                {
                    var released = await db.Database.ExecuteSqlInterpolatedAsync($@"
                        UPDATE SeatLocks SET Status = 'Released'
                        WHERE LockId = {lockId} AND Status = 'Locked'", stoppingToken);
                    if (released == 1)
                    {
                        await db.Database.ExecuteSqlInterpolatedAsync($@"
                            UPDATE offer_inventory.TransportOffers
                            SET SeatsAvailable = SeatsAvailable + {seatLock.NumberOfSeats}
                            WHERE Id = {seatLock.OfferId}", stoppingToken);
                    }
                }
                await transaction.CommitAsync(stoppingToken);
            }

            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
    }
}

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

            var expired = await db.SeatLocks
                .Where(s => s.Status == SeatLockStatus.Locked && s.ExpiresAt < DateTime.UtcNow)
                .ToListAsync();

            foreach (var s in expired)
                s.Release();

            await db.SaveChangesAsync();

            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
    }
}


using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Booking.Domain.Entities;
using Booking.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OfferInventory.Infrastructure.Data;
using OfferInventory.Domain.Entities;

namespace Booking.API.HostedServices
{
    /// <summary>
    /// 后台任务：随机模拟 Offer 的 price/availability 变更
    /// </summary>
    public class OfferChangeSimulator : BackgroundService
    {
        private readonly BookingDbContext       _bookingDb;
        private readonly AppDbContext           _invDb;
        private readonly ILogger<OfferChangeSimulator> _logger;
        private Guid[] _sampleOffers = Array.Empty<Guid>();

        public OfferChangeSimulator(
            BookingDbContext bookingDb,
            AppDbContext     invDb,
            ILogger<OfferChangeSimulator> logger)
        {
            _bookingDb = bookingDb;
            _invDb     = invDb;
            _logger    = logger;
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("[Simulator] Step 1: StartAsync 开始加载样本 OfferId");
            Console.WriteLine("[Simulator] Step 1: StartAsync 开始加载样本 OfferId");

            try
            {
                _sampleOffers = await _invDb.TransportOffers
                    .Select(t => t.Id)
                    .Take(5)
                    .ToArrayAsync(cancellationToken);

                _logger.LogInformation(
                    "[Simulator] Step 1: 已加载 {Count} 个 OfferId: {Ids}",
                    _sampleOffers.Length,
                    string.Join(",", _sampleOffers)
                );
                Console.WriteLine(
                    $"[Simulator] Step 1: 已加载 {_sampleOffers.Length} 个 OfferId: {string.Join(",", _sampleOffers)}"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Simulator] Step 1: 加载样本 OfferId 时出错");
                Console.WriteLine($"[Simulator] Step 1 Error: {ex.Message}");
                // 不抛，让服务继续启动，ExecuteAsync 会看到 sampleOffers 为空
            }

            await base.StartAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken ct)
        {
            var rnd = new Random();
            _logger.LogInformation("[Simulator] Step 2: ExecuteAsync 循环开始");
            Console.WriteLine("[Simulator] Step 2: ExecuteAsync 循环开始");

            while (!ct.IsCancellationRequested)
            {
                // 2.1 检查样本
                if (_sampleOffers.Length == 0)
                {
                    _logger.LogWarning("[Simulator] Step 2.1: 样本为空，10秒后重试");
                    Console.WriteLine("[Simulator] Step 2.1: 样本为空，10秒后重试");
                    await Task.Delay(10_000, ct);
                    continue;
                }

                // 2.2 随机延迟
                int delayMs = rnd.Next(15_000, 25_000);
                _logger.LogInformation("[Simulator] Step 2.2: 随机延迟 {Delay}ms", delayMs);
                Console.WriteLine($"[Simulator] Step 2.2: 随机延迟 {delayMs}ms");
                await Task.Delay(delayMs, ct);

                // 2.3 随机选 ID 并读取 TransportOffer
                var idx = rnd.Next(_sampleOffers.Length);
                var offerId = _sampleOffers[idx];
                _logger.LogInformation("[Simulator] Step 2.3: 准备修改 Offer {OfferId}", offerId);
                Console.WriteLine($"[Simulator] Step 2.3: 准备修改 Offer {offerId}");

                TransportOffer? transport = null;
                try
                {
                    transport = await _invDb.TransportOffers.FindAsync(new object[] { offerId }, ct);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[Simulator] Step 2.3 Error: 读取 TransportOffer 时异常");
                    Console.WriteLine($"[Simulator] Step 2.3 Error: {ex.Message}");
                    continue;
                }

                if (transport == null)
                {
                    _logger.LogWarning("[Simulator] Step 2.3: 找不到 TransportOffer {OfferId}", offerId);
                    Console.WriteLine($"[Simulator] Step 2.3: 找不到 TransportOffer {offerId}");
                    continue;
                }

                // 2.4 决定改哪个字段
                bool changePrice = rnd.Next(2) == 0;
                string field = changePrice ? "price" : "availability";
                string oldValue = changePrice
                    ? transport.PriceTotal.ToString("0.00")
                    : transport.SeatsAvailable.ToString();

                string newValue;
                if (changePrice)
                {
                    var nv = transport.PriceTotal * (decimal)(1 + (rnd.NextDouble() - 0.5) * 0.2);
                    transport.PriceTotal = Math.Round(nv, 2);
                    newValue = transport.PriceTotal.ToString("0.00");
                }
                else
                {
                    var nv = Math.Max(0, transport.SeatsAvailable + rnd.Next(-5, 6));
                    transport.SeatsAvailable = nv;
                    newValue = nv.ToString();
                }

                _logger.LogInformation(
                    "[Simulator] Step 2.4: {Field} 从 {Old} 改到 {New}",
                    field, oldValue, newValue
                );
                Console.WriteLine($"[Simulator] Step 2.4: {field} 从 {oldValue} 改到 {newValue}");

                // 2.5 写 OfferChanges
                try
                {
                    var change = new OfferChange
                    {
                        OfferId   = offerId,
                        ChangedAt = DateTime.Now,
                        Field     = field,
                        OldValue  = oldValue,
                        NewValue  = newValue
                    };
                    _bookingDb.OfferChanges.Add(change);
                    await _bookingDb.SaveChangesAsync(ct);
                    _logger.LogInformation(
                        "[Simulator] Step 2.5: 已写日志 OfferChange {ChangeId}",
                        change.Id
                    );
                    Console.WriteLine($"[Simulator] Step 2.5: 已写日志 OfferChange {change.Id}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[Simulator] Step 2.5 Error: 写变更日志失败");
                    Console.WriteLine($"[Simulator] Step 2.5 Error: {ex.Message}");
                }

                // 2.6 保存库存表
                try
                {
                    await _invDb.SaveChangesAsync(ct);
                    _logger.LogInformation("[Simulator] Step 2.6: 已保存库存改动");
                    Console.WriteLine("[Simulator] Step 2.6: 已保存库存改动");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[Simulator] Step 2.6 Error: 保存库存失败");
                    Console.WriteLine($"[Simulator] Step 2.6 Error: {ex.Message}");
                }
            }

            _logger.LogInformation("[Simulator] Step 2: 循环退出");
            Console.WriteLine("[Simulator] Step 2: 循环退出");
        }
    }
}

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OfferInventory.Application.Services
{
    /// <summary>
    /// 后台任务：批量爬取 7天 × N × (N-1) 城市组合数据
    /// 注册为 HostedService，随 ASP.NET Core 启动自动运行
    /// </summary>
    public sealed class FlixbusBatchCrawler : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<FlixbusBatchCrawler> _log;

        public FlixbusBatchCrawler(
            IServiceScopeFactory scopeFactory,
            ILogger<FlixbusBatchCrawler> log)
        {
            _scopeFactory = scopeFactory;
            _log = log;
        }

        private static readonly string[] _cityIds =
        {
            "40e19c59-8646-11e6-9066-549f350fcb0c", // Warsaw
            "40de6982-8646-11e6-9066-549f350fcb0c", // Gdańsk
            "40de7b94-8646-11e6-9066-549f350fcb0c", // Katowice
            "40de575f-8646-11e6-9066-549f350fcb0c", // Wroclaw
            "40e19dd6-8646-11e6-9066-549f350fcb0c", // Lodz
            "40de7eb5-8646-11e6-9066-549f350fcb0c", // Kraków
            "40de8b67-8646-11e6-9066-549f350fcb0c", // Poznań
            "40de9afa-8646-11e6-9066-549f350fcb0c", // Szczecin
            "d7687a0f-488f-49c7-821e-5a26640c5f86", // Lublin
            "b5b11801-0928-43c1-af2e-2f01acbc0eca"  // Białystok
        };

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var routes =
                from fromId in _cityIds
                from toId   in _cityIds
                where fromId != toId
                select (fromId, toId);

            foreach (var (fromId, toId) in routes)
            {
                for (var offset = 0; offset < 7; offset++)
                {
                    var date = DateTime.Today.AddDays(offset);
                    if (stoppingToken.IsCancellationRequested)
                        return;

                    _log.LogInformation("Crawling {Date} {From} ➜ {To}",
                        date.ToString("yyyy-MM-dd"),
                        fromId[..8], toId[..8]);

                    try
                    {
                        using var scope = _scopeFactory.CreateScope();
                        var crawler = scope.ServiceProvider.GetRequiredService<FlixbusCrawler>();
                        await crawler.CrawlAsync(fromId, toId, date);
                    }
                    catch (Exception ex)
                    {
                        _log.LogWarning(ex, "Crawl failed: {From} ➜ {To}", fromId[..8], toId[..8]);
                    }
                }
            }
        }
    }
}


using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using OfferInventory.Infrastructure.Data;

namespace OfferInventory.Application.Services;

/// <summary>
/// Periodically refreshes FlixBus offers for the configured cities.
/// </summary>
public sealed class FlixbusBatchCrawler : BackgroundService
{
    private static readonly string[] DefaultCityIds =
    {
        "40e19c59-8646-11e6-9066-549f350fcb0c", // Warsaw
        "40de6982-8646-11e6-9066-549f350fcb0c"  // Gdansk
    };

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<FlixbusBatchCrawler> _log;
    private readonly IConfiguration _configuration;

    public FlixbusBatchCrawler(
        IServiceScopeFactory scopeFactory,
        ILogger<FlixbusBatchCrawler> log,
        IConfiguration configuration)
    {
        _scopeFactory = scopeFactory;
        _log = log;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_configuration.GetValue("FlixbusCrawler:Enabled", true))
        {
            _log.LogInformation("FlixBus background crawler is disabled.");
            return;
        }

        var cityIds = _configuration
            .GetSection("FlixbusCrawler:CityIds")
            .Get<string[]>()?
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (cityIds is not { Length: >= 2 })
            cityIds = DefaultCityIds;

        var daysAhead = Math.Clamp(
            _configuration.GetValue("FlixbusCrawler:DaysAhead", 7), 1, 31);
        var refreshHours = Math.Max(
            _configuration.GetValue("FlixbusCrawler:RefreshHours", 12), 1);
        var retainPastDays = Math.Clamp(
            _configuration.GetValue("FlixbusCrawler:RetainPastDays", 1), 0, 30);
        var maxStoredOffers = Math.Clamp(
            _configuration.GetValue("FlixbusCrawler:MaxStoredOffers", 5000), 100, 100000);

        // Let migrations finish and the application start before making external calls.
        await Task.Delay(TimeSpan.FromSeconds(3), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            await CleanupAsync(daysAhead, retainPastDays, maxStoredOffers, stoppingToken);
            await CrawlAllAsync(cityIds, daysAhead, stoppingToken);
            await CleanupAsync(daysAhead, retainPastDays, maxStoredOffers, stoppingToken);
            await Task.Delay(TimeSpan.FromHours(refreshHours), stoppingToken);
        }
    }

    private async Task CleanupAsync(
        int daysAhead,
        int retainPastDays,
        int maxStoredOffers,
        CancellationToken stoppingToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var earliest = today.AddDays(-retainPastDays);
        var latest = today.AddDays(daysAhead - 1);

        var outsideWindow = await db.TransportOffers
            .Where(offer => offer.DepartureDate < earliest || offer.DepartureDate > latest)
            .ExecuteDeleteAsync(stoppingToken);

        var total = await db.TransportOffers.CountAsync(stoppingToken);
        var overflow = Math.Max(0, total - maxStoredOffers);
        var capped = 0;
        if (overflow > 0)
        {
            var ids = await db.TransportOffers
                .OrderBy(offer => offer.DepartureDate)
                .ThenBy(offer => offer.DepartureTime)
                .Select(offer => offer.Id)
                .Take(overflow)
                .ToListAsync(stoppingToken);

            capped = await db.TransportOffers
                .Where(offer => ids.Contains(offer.Id))
                .ExecuteDeleteAsync(stoppingToken);
        }

        if (outsideWindow + capped > 0)
        {
            _log.LogInformation(
                "FlixBus retention cleanup deleted {OutsideWindow} out-of-window and {Capped} over-limit offers.",
                outsideWindow, capped);
        }
    }

    private async Task CrawlAllAsync(
        IReadOnlyCollection<string> cityIds,
        int daysAhead,
        CancellationToken stoppingToken)
    {
        var inserted = 0;
        var failed = 0;

        foreach (var fromId in cityIds)
        foreach (var toId in cityIds)
        {
            if (string.Equals(fromId, toId, StringComparison.OrdinalIgnoreCase))
                continue;

            for (var offset = 0; offset < daysAhead; offset++)
            {
                stoppingToken.ThrowIfCancellationRequested();
                var date = DateTime.UtcNow.Date.AddDays(offset);

                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var crawler = scope.ServiceProvider.GetRequiredService<FlixbusCrawler>();
                    inserted += await crawler.CrawlAsync(fromId, toId, date);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    failed++;
                    _log.LogWarning(ex,
                        "FlixBus crawl failed for {Date}, {From} -> {To}",
                        date.ToString("yyyy-MM-dd"), fromId, toId);
                }
            }
        }

        _log.LogInformation(
            "FlixBus refresh completed: {Inserted} offers inserted, {Failed} requests failed.",
            inserted, failed);
    }
}

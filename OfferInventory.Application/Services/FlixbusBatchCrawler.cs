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
            "40de6094-8646-11e6-9066-549f350fcb0c" // "Bydgoszcz"
            "40de1ad1-8646-11e6-9066-549f350fcb0c"// "Prague",
            "40e1a0af-8646-11e6-9066-549f350fcb0c"// = "Brno",
            "40e0eb79-8646-11e6-9066-549f350fcb0c"// = "Ostrava",
            "40de8a5a-8646-11e6-9066-549f350fcb0c"// = "Plzeň",
            "40e2a266-8646-11e6-9066-549f350fcb0c"// = "Liberec",
            "40de8a5a-8646-11e6-9066-549f350fcb0c"// = "Olomouc",
            "40de6751-8646-11e6-9066-549f350fcb0c"// = "České Budějovice",
            "40e0c29f-8646-11e6-9066-549f350fcb0c"// = "Hradec Králové",
            "40d8f682-8646-11e6-9066-549f350fcb0c"// = "Berlin",
            "40d91e53-8646-11e6-9066-549f350fcb0c"// = "Hamburg",
            "40d901a5-8646-11e6-9066-549f350fcb0c"// = "Munich",
            "40d91025-8646-11e6-9066-549f350fcb0c"// = "Koeln",
            "40d90407-8646-11e6-9066-549f350fcb0c"// = "Frankfurt am Main",
            "40da3d5e-8646-11e6-9066-549f350fcb0c"// = "Essen",
            "40d90995-8646-11e6-9066-549f350fcb0c"// = "Stuttgart",
            "40da382b-8646-11e6-9066-549f350fcb0c"// = "Dortmund",
            "40d911c7-8646-11e6-9066-549f350fcb0c"// = "Duesseldorf",
            "40da6e70-8646-11e6-9066-549f350fcb0c"// = "Bremen",
            "40da4ac8-8646-11e6-9066-549f350fcb0c"// = "Hannover",
            "40d917f9-8646-11e6-9066-549f350fcb0c"// = "Leipzig",
            "40da79b3-8646-11e6-9066-549f350fcb0c"// = "Duisburg",
            "40d90d0f-8646-11e6-9066-549f350fcb0c"// = "Nuernberg",
            "40db219f-8646-11e6-9066-549f350fcb0c"// = "Dresden",
            "40da3a44-8646-11e6-9066-549f350fcb0c"// = "Bochum",
            "40da70aa-8646-11e6-9066-549f350fcb0c"// = "Wuppertal",
            "40dad33a-8646-11e6-9066-549f350fcb0c"// = "Bielefeld",
            "40dadbff-8646-11e6-9066-549f350fcb0c"// = "Bonn",
            "40d90c3a-8646-11e6-9066-549f350fcb0c"// = "Mannheim",
            "40d912c2-8646-11e6-9066-549f350fcb0c"// = "Karlsruhe",
            "40dd4112-8646-11e6-9066-549f350fcb0c"// = "Wiesbaden",
            "40dc47e2-8646-11e6-9066-549f350fcb0c"// = "Muenster",
            "40ddc67e-8646-11e6-9066-549f350fcb0c"// = "Gelsenkirchen",
            "40da8ddc-8646-11e6-9066-549f350fcb0c"// = "Aachen",
            "40da838e-8646-11e6-9066-549f350fcb0c"// = "Moenchengladbach",
            "40da3fd1-8646-11e6-9066-549f350fcb0c"// = "Augsburg",
            "40da6fab-8646-11e6-9066-549f350fcb0c"// = "Chemnitz",
            "40d928aa-8646-11e6-9066-549f350fcb0c"// = "Braunschweig",
            "40dbe90d-8646-11e6-9066-549f350fcb0c"// = "Krefeld",
            "40da5768-8646-11e6-9066-549f350fcb0c"// = "Halle",
            "40dbe253-8646-11e6-9066-549f350fcb0c"// = "Kiel",
            "40da54cb-8646-11e6-9066-549f350fcb0c"// = "Magdeburg",
            "40d902e6-8646-11e6-9066-549f350fcb0c"// = "Neue Neustadt",
            "40da7d07-8646-11e6-9066-549f350fcb0c"// = "Oberhausen",
            "40d8ff3b-8646-11e6-9066-549f350fcb0c"// = "Freiburg",
            "40dc0389-8646-11e6-9066-549f350fcb0c"// = "Luebeck",
            "40db4593-8646-11e6-9066-549f350fcb0c"// = "Erfurt",
            "40db8000-8646-11e6-9066-549f350fcb0c"// = "Hagen",
            "40da4248-8646-11e6-9066-549f350fcb0c"// = "Rostock",
            "40dbdcd2-8646-11e6-9066-549f350fcb0c"// = "Kassel",
            "40dc0e9e-8646-11e6-9066-549f350fcb0c"// = "Mainz",
            "40dcbfdd-8646-11e6-9066-549f350fcb0c"// = "Saarbruecken",
            "40dba27d-8646-11e6-9066-549f350fcb0c"// = "Herne",
            "40dc4560-8646-11e6-9066-549f350fcb0c"// = "Muelheim",
            "40dc63d2-8646-11e6-9066-549f350fcb0c"// = "Osnabrueck",
            "40da6d2c-8646-11e6-9066-549f350fcb0c"// = "Oldenburg",
            "40dc7203-8646-11e6-9066-549f350fcb0c"// = "Potsdam",
            "40d90575-8646-11e6-9066-549f350fcb0c"// = "Darmstadt",
            "40d915ac-8646-11e6-9066-549f350fcb0c"// = "Wuerzburg",
            "40dc7b76-8646-11e6-9066-549f350fcb0c"// = "Regensburg",
            "40dd5460-8646-11e6-9066-549f350fcb0c"// = "Wolfsburg",
            "40dbd358-8646-11e6-9066-549f350fcb0c"// = "Ingolstadt",
            "40dd1618-8646-11e6-9066-549f350fcb0c"// = "Ulm",
            "40da743c-8646-11e6-9066-549f350fcb0c"// = "Trier",
            "40d92538-8646-11e6-9066-549f350fcb0c"// = "Worms",
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


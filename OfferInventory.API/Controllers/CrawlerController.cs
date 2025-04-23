using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using OfferInventory.Application.Services;

namespace OfferInventory.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CrawlerController : ControllerBase
    {
        private readonly FlixbusCrawler _crawler;

        public CrawlerController(FlixbusCrawler crawler)
            => _crawler = crawler;

        /// <summary>
        /// 抓单日 A→B 班次并写库，返回写入数量
        /// GET /api/crawler/run?from=…&to=…[&date=yyyy-MM-dd]
        /// </summary>
        [HttpGet("run")]
        public async Task<IActionResult> Run(
            [FromQuery] string from,
            [FromQuery] string to,
            [FromQuery] DateTime? date)
        {
            var dt = date ?? DateTime.UtcNow.Date;
            var count = await _crawler.CrawlAsync(from, to, dt);
            return Ok(new {
                Message = $"Inserted {count} trip(s) from {from}→{to} on {dt:yyyy-MM-dd}",
                Count = count
            });
        }

        /// <summary>
        /// 仅打印 API 响应，不写库
        /// GET /api/crawler/debug?from=…&to=…[&date=yyyy-MM-dd]
        /// </summary>
        [HttpGet("debug")]
        public async Task<IActionResult> Debug(
            [FromQuery] string from,
            [FromQuery] string to,
            [FromQuery] DateTime? date)
        {
            var dt = date ?? DateTime.UtcNow.Date;
            var url = FlixbusCrawler.BuildFlixbusUrl(from, to, dt);
            var json = await _crawler.GetRawAsync(url);
            return Content(json, "application/json");
        }
    }
}

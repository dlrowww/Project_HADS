using Microsoft.AspNetCore.Mvc;
using OfferInventory.Application.Services;

namespace OfferInventory.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AdminController : ControllerBase
{
    private readonly FlixbusCrawler _crawler;

    public AdminController(FlixbusCrawler crawler) => _crawler = crawler;

    /// <summary>
    ///  手动触发一次爬取 —— 示例城市：华沙 ➜ 格但斯克，抓取明天的数据  
    ///  POST /api/admin/crawl
    /// </summary>
    /// 
    /// 
    /// 
    /// 
    [HttpPost("crawl")]
    public async Task<IActionResult> Crawl()
    {
        await _crawler.CrawlAsync(
            fromCityId: "40e19c59-8646-11e6-9066-549f350fcb0c",   // Warsaw
            toCityId  : "40de6982-8646-11e6-9066-549f350fcb0c",   // Gdańsk
            dateUtc   : DateTime.UtcNow.Date.AddDays(1));

        return Ok(new { message = "FlixBus offers crawled ✔" });
    }
}


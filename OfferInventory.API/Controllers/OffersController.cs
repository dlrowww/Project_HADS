using Microsoft.AspNetCore.Mvc;
using OfferInventory.Application.Interfaces;

namespace OfferInventory.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OffersController : ControllerBase
{
    private readonly IOfferService _offerService;
    public OffersController(IOfferService offerService) => _offerService = offerService;

    /// <summary>
    /// 生成随机假数据到内存库，例如：POST /api/offers/seed?count=20
    /// </summary>
    [HttpPost("seed")]
    public async Task<IActionResult> Seed(int count = 10)
    {
        var inserted = await _offerService.SeedAsync(count);
        return Ok(new { inserted });
    }

    /// <summary>
    /// 查询所有数据，例如：GET /api/offers
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var all = await _offerService.GetAllAsync();
        return Ok(all);
    }
}

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Gateway.API.Models;    // <-- 引入你自己在 Gateway.API 下定义的 DTO 命名空间

namespace Gateway.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OffersController : ControllerBase
    {
        private readonly HttpClient _search;

        public OffersController(IHttpClientFactory factory) =>
            _search = factory.CreateClient("search");

        [HttpGet("search")]
        public async Task<IActionResult> Search(
            [FromQuery] string origin,
            [FromQuery] string destination,
            [FromQuery] DateTime date,
            [FromQuery] string transport,
            [FromQuery] int people,
            [FromQuery] bool promotion)
        {
            // 构造转发给 Search.API 的查询字符串
            var qs = new QueryString()
                .Add("from",           origin)
                .Add("to",             destination)
                .Add("date",           date.ToString("yyyy-MM-dd"))
                .Add("transportType",  transport)
                .Add("numberOfPeople", people.ToString())
                .Add("promoOnly",      promotion.ToString().ToLower());

            // 1) 直接反序列化 Search.API 返回的 JSON 为 List<OfferDto>
            List<OfferDto>? offers;
            try
            {
                offers = await _search.GetFromJsonAsync<List<OfferDto>>(
                    $"/api/offers/search{qs}"
                );
            }
            catch (Exception ex)
            {
                // 如果反序列化失败，返回 502
                return StatusCode(
                    StatusCodes.Status502BadGateway,
                    $"Failed to parse Search.API response: {ex.Message}"
                );
            }

            if (offers is null)
                return StatusCode(
                    StatusCodes.Status502BadGateway,
                    "Search.API returned no data"
                );

            // 2) 原样返回（内容已是 JSON，框架会自动序列化）
            return Ok(offers);
        }
    }
}

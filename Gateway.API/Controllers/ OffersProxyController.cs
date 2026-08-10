
// Gateway.API/Controllers/OffersProxyController.cs
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Gateway.API.Controllers
{
    [ApiController]
    [Route("api/offers/{offerId:guid}/bookings")]
    public class OffersProxyController : ControllerBase
    {
        private readonly HttpClient     _client;
        private readonly IConfiguration _cfg;

        public OffersProxyController(IHttpClientFactory httpFactory, IConfiguration cfg)
        {
            // 从 Program.cs 注入的命名 HttpClient
            // 在 Program.cs 应有：
            // builder.Services.AddHttpClient("booking-api", c =>
            //     c.BaseAddress = new Uri(cfg["Services:BookingApi"]));
            _client = httpFactory.CreateClient("booking-api");
            _cfg    = cfg;
        }

        /// <summary>
        /// GET /api/offers/{offerId}/bookings/recent
        /// 转发到 Booking.API 的相同路由并返回原始 JSON
        /// </summary>
        [HttpGet("recent")]
        public async Task<IActionResult> GetRecent(Guid offerId)
        {
            // 构造 Booking.API 的路径
            var url = $"/api/offers/{offerId}/bookings/recent";

            // 发起 HTTP 请求
            var response = await _client.GetAsync(url);

            // 读取响应
            var json = await response.Content.ReadAsStringAsync();

            // 原样返回 JSON 给前端
            return new ContentResult
            {
                Content     = json,
                ContentType = "application/json",
                StatusCode  = (int)response.StatusCode
            };
        }
    }
}

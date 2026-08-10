using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authorization;
using System.Net.Http.Headers;

namespace Gateway.API.Controllers
{
    [ApiController]
    [Route("api/preferencestats")]
    [Authorize]
    public class PreferenceStatsProxyController : ControllerBase
    {
        private readonly IHttpClientFactory _httpFactory;
        private readonly IConfiguration     _cfg;

        public PreferenceStatsProxyController(
            IHttpClientFactory httpFactory,
            IConfiguration cfg)
        {
            _httpFactory = httpFactory;
            _cfg         = cfg;
        }

        /// <summary>
        /// 代理 GET /api/preferencestats/top 到 Booking.API
        /// </summary>
        [HttpGet("top")]
        public async Task<IActionResult> GetTop()
        {
            // 从配置读取 Booking 服务地址（appsettings.json 或 环境变量）
            var bookingBase = _cfg["Services:BookingApi"]?.TrimEnd('/');
            if (string.IsNullOrEmpty(bookingBase))
                return Problem("BookingApi 配置缺失", statusCode: 500);

            // 构造目标 URL
            var url = $"{bookingBase}/api/preferencestats/top";

            // 发起请求
            var client = _httpFactory.CreateClient();
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            if (Request.Headers.TryGetValue("Authorization", out var authorization))
                request.Headers.Authorization = AuthenticationHeaderValue.Parse(authorization.ToString());
            var resp = await client.SendAsync(request);

            // 顺序读取并转发状态码与内容
            var json = await resp.Content.ReadAsStringAsync();
            return new ContentResult
            {
                StatusCode  = (int)resp.StatusCode,
                ContentType = "application/json",
                Content     = json
            };
        }
    }
}

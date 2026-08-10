using System.Net.Http;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

[ApiController]
[Route("api/booking-proxy")]
public class BookingController : ControllerBase
{
    private readonly HttpClient _http;
    private readonly IConfiguration _cfg;

    public BookingController(IHttpClientFactory factory, IConfiguration cfg)
    {
        _http = factory.CreateClient();
        _cfg  = cfg;
    }

    [HttpPost]
    public async Task<IActionResult> PostBooking([FromBody] JsonElement body)
    {
        // ① 取配置并校验
        var baseUrl = _cfg["Services:BookingApi"];
        if (string.IsNullOrWhiteSpace(baseUrl))
            throw new InvalidOperationException("BookingApi address is not configured!");

        var url = $"{baseUrl.TrimEnd('/')}/api/booking";
        Console.WriteLine($"[Gateway] ➡  Forwarding request to Booking API: {url}");

        // ② 发送
        var content  = new StringContent(body.GetRawText(), Encoding.UTF8, "application/json");
        var response = await _http.PostAsync(url, content);

        Console.WriteLine($"[Gateway] ⬅  Booking API responded {response.StatusCode}");

        // ③ 把响应直接转给前端
        var respJson = await response.Content.ReadAsStringAsync();
        return new ContentResult
        {
            Content     = respJson,
            ContentType = "application/json",
            StatusCode  = (int)response.StatusCode
        };
    }
}

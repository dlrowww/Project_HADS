using System.Net.Http;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authorization;
using System.Net.Http.Headers;

[ApiController]
[Route("api/booking-proxy")]
[Authorize]
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
        using var request = new HttpRequestMessage(HttpMethod.Post, url) { Content = content };
        if (Request.Headers.TryGetValue("Authorization", out var authorization))
            request.Headers.Authorization = AuthenticationHeaderValue.Parse(authorization.ToString());
        var response = await _http.SendAsync(request);

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

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetBooking(Guid id)
    {
        var baseUrl = _cfg["Services:BookingApi"];
        if (string.IsNullOrWhiteSpace(baseUrl))
            throw new InvalidOperationException("BookingApi address is not configured!");

        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"{baseUrl.TrimEnd('/')}/api/booking/{id}");
        if (Request.Headers.TryGetValue("Authorization", out var authorization))
            request.Headers.Authorization = AuthenticationHeaderValue.Parse(authorization.ToString());

        var response = await _http.SendAsync(request);
        var responseBody = await response.Content.ReadAsStringAsync();
        return new ContentResult
        {
            Content = responseBody,
            ContentType = "application/json",
            StatusCode = (int)response.StatusCode
        };
    }
}

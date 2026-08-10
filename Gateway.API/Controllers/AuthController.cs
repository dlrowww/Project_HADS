using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System;


namespace Gateway.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IHttpClientFactory _factory;

    public AuthController(IHttpClientFactory factory)
    {
        _factory = factory;
    }

    // ---------- 注册 ----------
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] object dto)
    {
        var client = _factory.CreateClient("user");
        var resp = await client.PostAsJsonAsync("/api/user/register", dto);
        return await Passthrough(resp);
    }

    // ---------- 登录 ----------
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] object dto)
    {
        try
        {
            var client = _factory.CreateClient("user");
            var resp = await client.PostAsJsonAsync("/api/user/login", dto);

            var result = await resp.Content.ReadAsStringAsync();
            return StatusCode((int)resp.StatusCode, result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Gateway Error: {ex.Message}");
        }
    }
    // ---------- 获取用户信息 ----------
    [HttpGet("info")]
    public async Task<IActionResult> Info()
    {
        var client = _factory.CreateClient("user");

        var req = new HttpRequestMessage(HttpMethod.Get, "/api/user/info");
        if (Request.Headers.TryGetValue("Authorization", out var token))
            req.Headers.Authorization = AuthenticationHeaderValue.Parse(token.ToString());

        var resp = await client.SendAsync(req);
        return await Passthrough(resp);
    }

    // ---------- 共用包装 ----------
    private static async Task<IActionResult> Passthrough(HttpResponseMessage resp)
    {
        var body = await resp.Content.ReadAsStringAsync();
        return new ContentResult
        {
            StatusCode = (int)resp.StatusCode,
            Content = body,
            ContentType = resp.Content.Headers.ContentType?.ToString() ?? "application/json"
        };
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;       // QueryString
using System;
using System.Net.Http;                 // HttpClient
using System.Net.Http.Json;            // ReadFromJsonAsync
using System.Threading;
using System.Threading.Tasks;


namespace Gateway.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OffersController : ControllerBase
    {
        private readonly HttpClient _searchClient;

        public OffersController(IHttpClientFactory factory)
        {
            // 一定要和上面 Program.cs 里 AddHttpClient("search", ...) 对应
            _searchClient = factory.CreateClient("search");
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search(
            [FromQuery] int    people,
            [FromQuery] string transport,
            [FromQuery] DateTime date,
            [FromQuery] bool   promotion,
            [FromQuery] string origin,
            [FromQuery] string destination,
            CancellationToken  ct)
        {
            // 调试日志：确认 QueryString 正确
            Console.WriteLine($"🔍 Incoming Query: people={people}, transport={transport}, date={date:yyyy-MM-dd}, promotion={promotion}, origin={origin}, destination={destination}");

            // 拼接 ?a=1&b=2...
            var qs = new QueryString()
                .Add("people",      people.ToString())
                .Add("transport",   transport)
                .Add("date",        date.ToString("yyyy-MM-dd"))
                .Add("promotion",   promotion.ToString().ToLower())
                .Add("origin",      origin)
                .Add("destination", destination);

            // 转发给 Search.API
            var resp = await _searchClient.GetAsync($"/api/offers/search{qs}", ct);
            if (!resp.IsSuccessStatusCode)
                return StatusCode((int)resp.StatusCode, "Search.API returned error");

            // 直接 Passthrough JSON
            var payload = await resp.Content.ReadFromJsonAsync<object>(cancellationToken: ct);
            return Ok(payload);
        }
    }
}

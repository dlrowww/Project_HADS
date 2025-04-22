// Search.API/Controllers/OffersController.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Search.API.Models;

namespace Search.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OffersController : ControllerBase
{
    private readonly HttpClient _inventoryClient;

    public OffersController(IHttpClientFactory factory)
    {
        _inventoryClient = factory.CreateClient("offerInventory");
    }

    [HttpGet("search")]
    public async Task<IActionResult> SearchAsync(
        [FromQuery] int people,
        [FromQuery] string? transport,
        [FromQuery] DateTime date,
        [FromQuery] bool promotion = false,
        [FromQuery] string? origin = null,
        [FromQuery] string? destination = null,
        CancellationToken ct = default
    )
    {
        // ✅ 添加这行日志调试输出
        Console.WriteLine($"🔍 Incoming Query: people={people}, transport={transport}, date={date}, promotion={promotion}, origin={origin}, destination={destination}");

        // 向 OfferInventory 拉取全部
        var offers = await _inventoryClient.GetFromJsonAsync<List<TransportOfferDto>>("/api/offers", ct);
        if (offers == null || offers.Count == 0)
        {
            return Ok(Array.Empty<TransportOfferDto>());
        }

        // 本地筛选
        var query = offers.AsQueryable();

        if (!string.IsNullOrWhiteSpace(transport))
            query = query.Where(x => x.TransportType.Equals(transport, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(origin))
            query = query.Where(x => x.FromCity == origin);

        if (!string.IsNullOrWhiteSpace(destination))
            query = query.Where(x => x.ToCity == destination);

        query = query.Where(x => x.DepartureTime.Date == date.Date);
        query = query.Where(x => x.AvailableSeats >= people);

        Console.WriteLine($"🎯 Match count: {query.Count()}");

        Console.WriteLine("🚀 Returning offers:");
        foreach (var o in query)
        {
           Console.WriteLine($"🚌 {o.FromCity} -> {o.ToCity}, {o.TransportType}, Seats: {o.AvailableSeats}");
        }



        return Ok(query.ToList());
    }
}

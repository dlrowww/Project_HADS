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
using Microsoft.EntityFrameworkCore;
using OfferInventory.Infrastructure.Data;  // 你的 AppDbContext
// AppDbContext 所在命名空间

namespace Search.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OffersController : ControllerBase
    {
        private readonly AppDbContext _db;
        public OffersController(AppDbContext db) => _db = db;

        [HttpGet("search")]
        public async Task<IActionResult> SearchOffers(
            [FromQuery] string from,
            [FromQuery] string to,
            [FromQuery] DateOnly date,
            [FromQuery] string transportType,
            [FromQuery] int numberOfPeople,
            [FromQuery] bool promoOnly)
        {
            var q = _db.TransportOffers.AsQueryable();

            q = q.Where(o => o.FromCity == from && o.ToCity == to);
            q = q.Where(o => o.DepartureDate == date);
            if (!string.IsNullOrWhiteSpace(transportType))
                q = q.Where(o => o.MeansOfTransport == transportType);
            if (promoOnly)
                q = q.Where(o => o.IsPromoted);
            q = q.Where(o => o.SeatsAvailable >= numberOfPeople);

            var list = await q
                .Select(o => new TransportOfferDto
                {
                    ShortId        = o.ShortId,       // ← 新增映射
                    //Id             = o.Id,
                    DepartureTime  = o.DepartureTime,
                    ArrivalTime    = o.ArrivalTime,
                    Price          = o.PriceTotal,
                    Currency       = o.Currency,
                    Provider       = o.Provider,
                    FromCity       = o.FromCity,
                    ToCity         = o.ToCity,
                    AvailableSeats = o.SeatsAvailable,
                    IsPromoted     = o.IsPromoted,
                    Transport      = o.MeansOfTransport,
                    DurationHours = o.DurationHours,
                    DurationMinutes = o.DurationMinutes

                })
                .ToListAsync();

            return Ok(list);
        }
    }
}

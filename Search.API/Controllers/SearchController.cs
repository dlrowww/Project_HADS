using Microsoft.AspNetCore.Mvc;
using Search.API.Models;
using System.Net.Http.Json;

namespace Search.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SearchController : ControllerBase
    {
        private readonly HttpClient _httpClient;

        public SearchController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("OfferInventory");
        }

        [HttpPost]
        public async Task<IActionResult> Search([FromBody] SearchRequest request)
        {
            var offers = await _httpClient.GetFromJsonAsync<List<TransportOffer>>("/api/offers");

            if (offers == null)
                return StatusCode(500, "获取交通产品失败");

            var result = offers.Where(o =>
                o.Origin.Equals(request.Origin, StringComparison.OrdinalIgnoreCase) &&
                o.Destination.Equals(request.Destination, StringComparison.OrdinalIgnoreCase) &&
                (!request.DepartureDate.HasValue || o.DepartureTime.Date == request.DepartureDate.Value.Date)
            ).ToList();

            return Ok(result);
        }
    }
}


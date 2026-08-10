using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Gateway.API.Controllers
{
    [ApiController]
    [Route("api/offers/{offerId:guid}/changes")]
    public class OfferChangesProxyController : ControllerBase
    {
        private readonly HttpClient _booking;
        private readonly ILogger<OfferChangesProxyController> _logger;

        public OfferChangesProxyController(IHttpClientFactory factory, IConfiguration config, ILogger<OfferChangesProxyController> logger)
        {
            _booking = factory.CreateClient("booking-api");
            if (_booking.BaseAddress == null)
            {
                var baseAddr = config["Services:BookingApi"];
                if (string.IsNullOrWhiteSpace(baseAddr))
                {
                    throw new InvalidOperationException("Missing configuration: Services:BookingApi");
                }
                _booking.BaseAddress = new Uri(baseAddr);
                logger.LogInformation("[Gateway.API] Set booking-api BaseAddress to {BaseAddress}", baseAddr);
                Console.WriteLine($"[Gateway.API] BaseAddress set: {baseAddr}");
            }
            _logger = logger;
        }

        [HttpGet("recent")]
        public async Task<IActionResult> GetRecent(Guid offerId)
        {
            _logger.LogInformation("[Gateway.API] Proxying request for OfferId: {OfferId}", offerId);
            Console.WriteLine($"[Gateway.API] Proxy GET /api/offers/{offerId}/changes/recent");

            var resp = await _booking.GetAsync($"/api/offers/{offerId}/changes/recent");
            _logger.LogInformation("[Gateway.API] Received status code {StatusCode} from Booking API", resp.StatusCode);
            Console.WriteLine($"[Gateway.API] Response status: {(int)resp.StatusCode}");

            var json = await resp.Content.ReadAsStringAsync();
            Console.WriteLine($"[Gateway.API] Response body length: {json.Length}");
            
            return new ContentResult
            {
                StatusCode = (int)resp.StatusCode,
                ContentType = "application/json",
                Content = json
            };
        }
    }
}

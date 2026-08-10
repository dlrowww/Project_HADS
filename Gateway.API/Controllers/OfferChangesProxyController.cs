using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using System.Net.Http.Headers;

namespace Gateway.API.Controllers
{
    [ApiController]
    [Route("api/offers/{offerId:guid}/changes")]
    [Authorize]
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

            using var request = new HttpRequestMessage(HttpMethod.Get, $"/api/offers/{offerId}/changes/recent");
            if (Request.Headers.TryGetValue("Authorization", out var authorization))
                request.Headers.Authorization = AuthenticationHeaderValue.Parse(authorization.ToString());
            var resp = await _booking.SendAsync(request);
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

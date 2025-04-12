using Microsoft.AspNetCore.Mvc;
using OfferInventory.Application.Services;
using OfferInventory.Domain.Entities;

namespace OfferInventory.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OffersController : ControllerBase
    {
        private readonly IOfferService _offerService;

        public OffersController(IOfferService offerService)
        {
            _offerService = offerService;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var offers = await _offerService.GetAllOffersAsync();
            return Ok(offers);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] TransportOffer offer)
        {
            var result = await _offerService.AddOfferAsync(offer);
            return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
        }
    }
}

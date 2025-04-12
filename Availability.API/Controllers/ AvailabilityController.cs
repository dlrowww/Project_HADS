using Microsoft.AspNetCore.Mvc;
using Availability.Application.Services;
using System;
using System.Threading.Tasks;

namespace Availability.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AvailabilityController : ControllerBase
    {
        private readonly IAvailabilityService _availabilityService;

        public AvailabilityController(IAvailabilityService availabilityService)
        {
            _availabilityService = availabilityService;
        }

        [HttpGet("{offerId}")]
        public async Task<IActionResult> Get(Guid offerId)
        {
            var result = await _availabilityService.GetAvailabilityAsync(offerId);
            return Ok(result);
        }
    }
}

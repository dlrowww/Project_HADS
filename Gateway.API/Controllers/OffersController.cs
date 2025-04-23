using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net;
using Microsoft.AspNetCore.Http;
using System;

namespace Gateway.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OffersController : ControllerBase
    {
        private readonly HttpClient _search;

        public OffersController(IHttpClientFactory factory) =>
            _search = factory.CreateClient("search");

        [HttpGet("search")]
        public async Task<IActionResult> Search(
            [FromQuery] string origin,
            [FromQuery] string destination,
            [FromQuery] DateTime date,
            [FromQuery] string transport,
            [FromQuery] int people,
            [FromQuery] bool promotion)
        {
            var qs = new QueryString()
                .Add("from",          origin)
                .Add("to",            destination)
                .Add("date",          date.ToString("yyyy-MM-dd"))
                .Add("transportType", transport)
                .Add("numberOfPeople",people.ToString())
                .Add("promoOnly",     promotion.ToString().ToLower());

            var resp = await _search.GetAsync($"/api/offers/search{qs}");
            if (!resp.IsSuccessStatusCode)
                return StatusCode((int)resp.StatusCode, "Search.API error");

            var json = await resp.Content.ReadAsStringAsync();
            return Content(json, "application/json");
        }
    }
}

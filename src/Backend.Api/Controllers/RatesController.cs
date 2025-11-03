using Backend.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;


namespace Backend.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RatesController : ControllerBase
    {
        private readonly IRateProvider _rateProvider;
        public RatesController(IRateProvider rateProvider) => _rateProvider = rateProvider;


        [HttpGet("{baseCurrency}")]
        public async Task<IActionResult> Get(string baseCurrency)
        {
            var rates = await _rateProvider.GetRatesAsync(baseCurrency);
            return Ok(new { Base = baseCurrency.ToUpperInvariant(), Rates = rates });
        }


        [HttpGet("currencies")]
        public async Task<IActionResult> GetCurrencies() => Ok(await _rateProvider.GetCurrenciesAsync());
    }
}
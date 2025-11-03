using Backend.Application.Dtos;
using Backend.Application.Interfaces;
using Backend.Application.Services;
using Microsoft.AspNetCore.Mvc;


namespace Backend.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ConversionController : ControllerBase
    {
        private readonly ConvertCurrencyService _service;
        private readonly IConversionRepository _repo;


        public ConversionController(ConvertCurrencyService service, IConversionRepository repo)
        {
            _service = service;
            _repo = repo;
        }


        [HttpPost]
        public async Task<IActionResult> Post([FromBody] ConversionRequestDto req)
        {
            var result = await _service.ConvertAsync(req);
            return CreatedAtAction(nameof(GetHistory), new { userId = result.UserId }, result);
        }


        [HttpGet("history")]
        public async Task<IActionResult> GetHistory([FromQuery] string? userId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var uid = userId ?? "hardcoded-user";
            var res = await _repo.GetHistoryAsync(uid, page, pageSize);
            return Ok(res);
        }
    }
}
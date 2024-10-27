using Microsoft.AspNetCore.Mvc;
using WebApplication16.Services;
using WebApplication16.models;

namespace WebApplication16.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ProtocolController : ControllerBase
    {
        private readonly IProtocolService _protocolService;

        public ProtocolController(IProtocolService protocolService)
        {
            _protocolService = protocolService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProtocolEntry>>> GetAll()
        {
            try
            {
                var entries = await _protocolService.GetAllAsync();
                return Ok(entries);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return BadRequest();
            }
        }

        [HttpPost]
        public async Task<ActionResult> PostProtocolEntry(ProtocolEntry entry)
        {
            try
            {
                await _protocolService.AddAsync(entry);
                return Ok();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return BadRequest();
            }
        }
    }
}

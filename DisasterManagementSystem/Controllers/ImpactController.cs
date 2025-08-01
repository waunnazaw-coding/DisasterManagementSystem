using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DisasterManagementSystem_Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ImpactController : ControllerBase
    {
        private readonly IImpactService _service;

        public ImpactController(IImpactService service)
        {
            _service = service;
        }

        // POST: api/impact/submit-multiple
        [HttpPost("submit-multiple")]
        public async Task<IActionResult> SubmitMultiple([FromBody] List<ImpactCreateDto> impacts)
        {
            if (impacts == null || impacts.Count == 0)
                return BadRequest(new { isSuccess = false, message = "No impact data provided." });

            await _service.CreateImpactsAsync(impacts);
            return Ok(new { isSuccess = true, message = "Impacts reported successfully." });
        }

        // GET: api/impact
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var impacts = await _service.GetAllAsync();
            return Ok(impacts);
        }

        // GET: api/impact/by-event/{disasterEventId}
        [HttpGet("by-event/{disasterEventId}")]
        public async Task<IActionResult> GetByDisasterEvent(int disasterEventId)
        {
            var impacts = await _service.GetByDisasterEventAsync(disasterEventId);
            return Ok(impacts);
        }

        // GET: api/impact/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var impact = await _service.GetByIdAsync(id);
            if (impact == null)
                return NotFound(new { isSuccess = false, message = "Impact not found." });

            return Ok(impact);
        }
    }
}

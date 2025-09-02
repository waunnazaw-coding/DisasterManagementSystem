using CloudinaryDotNet.Actions;
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
        [HttpGet("all-impacts")]
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

        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] ImpactUpdateStatusDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Status))
                return BadRequest(new { isSuccess = false, message = "Status is required." });

            var result = await _service.UpdateImpactStatusAsync(id, dto.Status);

            if (!result.IsSuccess)
                return BadRequest(new { isSuccess = false });

            return Ok(new { isSuccess = true, message = $"Impact status updated to {dto.Status}" });
        }


    }
}

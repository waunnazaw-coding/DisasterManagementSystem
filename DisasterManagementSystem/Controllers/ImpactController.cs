using Microsoft.AspNetCore.Mvc;
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

        [HttpPost("submit-multiple")]
        public async Task<IActionResult> SubmitMultiple([FromBody] List<ImpactCreateDto> impacts)
        {
            if (impacts == null || !impacts.Any())
                return BadRequest(new { isSuccess = false, message = "No impact data provided." });

            await _service.CreateImpactsAsync(impacts);
            return Ok(new { isSuccess = true, message = "Impacts reported successfully." });
        }
    }

}
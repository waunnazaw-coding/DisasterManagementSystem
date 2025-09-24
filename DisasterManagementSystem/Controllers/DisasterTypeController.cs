using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace DisasterManagementSystem_Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DisasterTypeController : ControllerBase
    {
        private readonly IDisasterTypeService _service;

        public DisasterTypeController(IDisasterTypeService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllAsync();
            if (!result.IsSuccess)
                return NotFound(result.Message);

            return Ok(result.Data);
        }
    }
}

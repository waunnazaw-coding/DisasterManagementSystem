using DisasterManagementSystem_Data.Models;
using DisasterManagementSystem_Data.Repositories.Interfaces;
using DisasterManagementSystem_Services.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DisasterManagementSystem_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GdacsDisasterEventController : ControllerBase
    {
        private readonly IGdacsDisasterService _service;

        public GdacsDisasterEventController(IGdacsDisasterService service)
        {
            _service = service;
        }

        // GET api/disasters
        [HttpGet]
        public async Task<ActionResult<IEnumerable<GdacsdisasterEvent>>> GetAllDisasters()
        {
            var disasters = await _service.GetAllEventsAsync();
            return Ok(disasters);
        }

        [HttpGet("week")]
        public async Task<IActionResult> GetEventsForCurrentWeek()
        {
            var events = await _service.GetEventsForCurrentWeekAsync();
            return Ok(events);
        }

        [HttpGet("today")]
        public async Task<IActionResult> GetTodaysEvents()
        {
            var events = await _service.GetTodaysEventsAsync();
            return Ok(events);
        }
    }
}

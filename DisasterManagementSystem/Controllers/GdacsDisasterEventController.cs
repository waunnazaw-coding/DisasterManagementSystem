using DisasterManagementSystem_Data.Models;
using DisasterManagementSystem_Data.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DisasterManagementSystem_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GdacsDisasterEventController : ControllerBase
    {
        private readonly IGdacsDisasterRepository _repository;

        public GdacsDisasterEventController(IGdacsDisasterRepository repository)
        {
            _repository = repository;
        }

        // GET api/disasters
        [HttpGet]
        public async Task<ActionResult<IEnumerable<GdacsdisasterEvent>>> GetAllDisasters()
        {
            var disasters = await _repository.GetAllAsync();
            return Ok(disasters);
        }
    }
}

using DisasterManagementSystem_Data.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DisasterManagementSystem_Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DisasterEventsController : ControllerBase
    {
        private readonly IGdacsDisasterRepository _repository;
        public DisasterEventsController(IGdacsDisasterRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var allEvents = await _repository.GetAllAsync();
            return Ok(allEvents);
        }
    }

}

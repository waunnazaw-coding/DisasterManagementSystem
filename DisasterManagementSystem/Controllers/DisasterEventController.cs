using DisasterManagementSystem_Data.Repositories.Interfaces;
using DisasterManagementSystem_Services.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DisasterManagementSystem_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DisasterEventController : ControllerBase
    {
        private readonly IDisasterEventService _eventService;
   
        public  DisasterEventController(IDisasterEventService eventService)
        {
            _eventService = eventService;
        }
        [HttpGet("all")]
        public async Task<IResult> GetAll()
        {
            var result = await _eventService.GetAllAsync();
            return result.Execute();
        }

    }
}

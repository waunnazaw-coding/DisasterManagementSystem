using System.Security.Claims;
using DisasterManagementSystem_Data.Models;
using DisasterManagementSystem_Services.Models;
using DisasterManagementSystem_Services.Models.LocationDtos;
using DisasterManagementSystem_Services.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace DisasterManagementSystem_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DisasterEventController : ControllerBase
    {
        private readonly IDisasterEventService _eventService;

        public DisasterEventController(IDisasterEventService eventService)
        {
            _eventService = eventService;
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _eventService.GetAllAsync();
            if (result.IsSuccess)
                return Ok(new { isSuccess = true, data = result.Data });
            return BadRequest(result.Message);
        }
        [HttpGet("all-active")]
        public async Task<IActionResult> GetAllActive()
        {
            var result = await _eventService.GetAllActiveAsync();
            if (result.IsSuccess)
                return Ok(new { isSuccess = true, data = result.Data });
            return BadRequest(result.Message);
        }

        [HttpGet("withlocation/{id}")]
        public async Task<IActionResult> GetByIdWithLocation(int id)
        {
            var result = await _eventService.GetByIdWithLocationAsync(id);

            if (!result.IsSuccess)
                return NotFound(new { message = result.Message });

            return Ok(result.Data);
        }

        [HttpGet("all-with-impacts")]
        public async Task<IActionResult> GetAllWithImpacts()
        {
            var data = await _eventService.GetAllWithAffectedPeopleAsync();
            return Ok(new { isSuccess = true, data });
        }

        [HttpGet("active-count")]
        public async Task<IActionResult> GetActiveCount()
        {
            var result = await _eventService.GetActiveCountAsync();
            if (result.IsSuccess)
                return Ok(result.Data);

            return NotFound(new { message = result.Message });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _eventService.GetByIdAsync(id);
            if (result.IsSuccess)
            {
                return Ok(new { isSuccess = true, data = result.Data });
            }
            return NotFound(new { isSuccess = false, message = result.Message });
        }


        [HttpPost("submit-form")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> SubmitForm([FromForm] EventFormCreateDto dto)
        {
            var result = await _eventService.AddEventFormAsync(dto);
            if (result.IsSuccess)
                return Ok(result);
            return BadRequest(result.Message);
        }

        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateEvent(int id, [FromForm] EventFormUpdateDto dto)
        {
            if (id != dto.Id)
                return BadRequest("Event ID mismatch.");

            var result = await _eventService.UpdateEventFormAsync(dto);

            if (!result.IsSuccess)
                return BadRequest(result.Message);

            return Ok(result.Data);
        }

        [HttpDelete("delete/{id}")]
        public async Task<IResult> Delete(int id)
        {
            var result = await _eventService.DeleteAsync(id);
            return result.Execute();
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchEvents([FromQuery] string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return BadRequest("Search term is required.");

            var events = await _eventService.SearchByNameAsync(name);
            return Ok(events);
        }

        [HttpGet("update/{id}")]
        public async Task<IActionResult> GetUpdateDto(int id)
        {
            var result = await _eventService.GetByIdForUpdateAsync(id);
            if (result.IsSuccess)
            {
                return Ok(new { isSuccess = true, data = result.Data });
            }
            return NotFound(new { isSuccess = false, message = result.Message });
        }


    }
}
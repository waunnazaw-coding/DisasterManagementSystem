using System.Security.Claims;
using DisasterManagementSystem_Data.Models;
using DisasterManagementSystem_Data.Service;
using DisasterManagementSystem_Services.Models;
using DisasterManagementSystem_Services.Models.LocationDtos;
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


        [HttpGet("{id}")]
        public async Task<IResult> GetById(int id)
        {
            var result = await _eventService.GetByIdAsync(id);
            return result.Execute();
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
        public async Task<IResult> Update(int id, [FromBody] DisasterEvent dto)
        {
            if (id != dto.Id)
                return Results.BadRequest("ID mismatch");

            var result = await _eventService.UpdateAsync(dto);
            return result.Execute();
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

    }
}
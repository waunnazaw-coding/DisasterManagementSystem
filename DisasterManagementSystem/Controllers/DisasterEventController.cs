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
        public async Task<IResult> GetAll()
        {
            var result = await _eventService.GetAllAsync();
            return result.Execute();
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
            if (!ModelState.IsValid)
                return BadRequest(Result<string>.Failure("Invalid form data"));

            var result = await _eventService.AddEventFormAsync(dto);

            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
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
    }
}
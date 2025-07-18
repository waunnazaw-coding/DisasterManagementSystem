using DisasterManagementSystem_Data.Models;
using DisasterManagementSystem_Services.Models;
using DisasterManagementSystem_Services.Models.LocationDtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace DisasterManagementSystem_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DisasterReportController : ControllerBase
    {
        private readonly IDisasterReportService _reportService;

        public DisasterReportController(IDisasterReportService reportService)
        {
            _reportService = reportService;
        }

        [HttpGet("all")]
        public async Task<IResult> GetAll()
        {
            var result = await _reportService.GetAllAsync();
            return result.Execute();
        }

        [HttpGet("{id}")]
        public async Task<IResult> GetById(int id)
        {
            var result = await _reportService.GetByIdAsync(id);
            return result.Execute();
        }

        [HttpPost("submit-form")]
        public async Task<IActionResult> SubmitForm([FromBody] FormCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(Result<string>.Failure("Invalid form data"));

            var result = await _reportService.AddFormAsync(dto);

            if (!result.IsSuccess)
                return BadRequest(result);
                
            return Ok(result);
        }

        [HttpPut("update/{id}")]
        public async Task<IResult> Update(int id, [FromBody] DisasterReportUpdateDto dto)
        {
            if (id != dto.Id)
                return Results.BadRequest("ID mismatch");

            var result = await _reportService.UpdateAsync(dto);
            return result.Execute();
        }
        [HttpDelete("delete/{id}")]
        public async Task<IResult> Delete(int id)
        {
            var result = await _reportService.DeleteAsync(id);
            return result.Execute();
        }
    }
}
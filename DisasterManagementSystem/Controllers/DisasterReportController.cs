using System.Security.Claims;
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

        [HttpPost("survey")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Create([FromForm] ReportImpactCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(Result<string>.Failure("Invalid form data"));

            // Try to extract UserId from token claims
            var userIdClaim = User.FindFirst("sub") ?? User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            {
                // Fallback to UnknownUserId
                userId = Guid.Empty;
            }

            dto.UserId = userId;

            var result = await _reportService.CreateAsync(dto);

            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPost("submit-form")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> SubmitForm([FromForm] FormCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(Result<string>.Failure("Invalid form data"));


            var result = await _reportService.AddFormAsync(dto);

            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }

        //[HttpPost("survey")]
        //[Consumes("multipart/form-data")]
        //public async Task<IActionResult> Create([FromForm] ReportImpactCreateDto dto)
        //{
        //    if (!ModelState.IsValid)
        //        return BadRequest(Result<string>.Failure("Invalid form data"));

        //    // Get UserId from claims
        //    var userIdClaim = User.FindFirst("sub") ?? User.FindFirst(ClaimTypes.NameIdentifier);
        //    if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
        //    {
        //        return Unauthorized(Result<string>.Failure("User ID not found in token"));
        //    }

        //    dto.UserId = userId;

        //    var result = await _reportService.CreateAsync(dto);

        //    if (!result.IsSuccess)
        //        return BadRequest(result);

        //    return Ok(result);
        //}

        [HttpPut("update-form/{id}")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UpdateForm(int id, [FromForm] FormUpdateDto dto)
        {
            if (id != dto.Id)
                return BadRequest(Result<string>.Failure("ID mismatch"));

            if (!ModelState.IsValid)
                return BadRequest(Result<string>.Failure("Invalid form data"));

            var result = await _reportService.UpdateFormAsync(dto);

            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }


        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _reportService.DeleteAsync(id);

            if (!result.IsSuccess)
            {
                if (result.IsNotFoundError)
                    return NotFound(result);
                else
                    return BadRequest(result);
            }

            return Ok(result);
        }

        // ---------------- Approve a report ----------------
        [HttpPost("approve/{id}")]
        public async Task<IActionResult> Approve(int id)
        {
            var result = await _reportService.ApproveAsync(id);

            if (!result.IsSuccess)
                return BadRequest(new { message = result.Message });

            return Ok(new { message = result.Message });
        }

        // ---------------- Disapprove a report ----------------
        [HttpPost("disapprove/{id}")]
        public async Task<IActionResult> Disapprove(int id)
        {
            var result = await _reportService.DisapproveAsync(id);

            if (!result.IsSuccess)
                return BadRequest(new { message = result.Message });

            return Ok(new { message = result.Message });
        }

        // --------------- UnDisapprove --------------------
        [HttpPost("unreject/{reportId}")]
        public async Task<IActionResult> UnrejectReport(int reportId)
        {
            var result = await _reportService.UnrejectReportAsync(reportId);
            if (result.IsSuccess)
                return Ok(result);

            return BadRequest(result);
        }


        // ---------------- Mark as Checked ----------------
        [HttpPost("checked/{id}")]
        public async Task<IActionResult> MarkAsChecked(int id)
        {
            var result = await _reportService.MarkAsCheckedAsync(id);

            if (!result.IsSuccess)
                return BadRequest(new { message = result.Message });

            return Ok(new { message = result.Message });
        }

        // ---------------- Mark as Fake ----------------
        [HttpPost("fake/{id}")]
        public async Task<IActionResult> MarkAsFake(int id)
        {
            var result = await _reportService.MarkAsFakeAsync(id);

            if (!result.IsSuccess)
                return BadRequest(new { message = result.Message });

            return Ok(new { message = result.Message });
        }

        // ---------------- Get rejected report reminders for Admin dashboard ----------------
        [HttpGet("will-delete-reminders")]
        public async Task<IResult> GetWillDeleteReminders()
        {
            var result = await _reportService.GetWillDeleteRemindersAsync();
            return result.Execute();
        }

    }
}
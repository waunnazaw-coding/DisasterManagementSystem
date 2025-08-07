using DisasterManagementSystem_Services.Models;
using DisasterManagementSystem_Services.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DisasterManagementSystem_Api.Controllers
{
    [Authorize(Roles = "Admin,ReliefTeam")]
    [Route("api/[controller]")]
    [ApiController]
    public class RequestAssignmentsController : ControllerBase
    {
        private readonly IRequestAssignmentService _assignmentService;

        public RequestAssignmentsController(IRequestAssignmentService assignmentService)
        {
            _assignmentService = assignmentService;
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IResult> CreateAssignment([FromBody] CreateRequestAssignmentDto dto)
        {
            if (!ModelState.IsValid)
                return Results.BadRequest(ModelState);

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userId, out Guid adminId))
                return Results.Unauthorized();

            var result = await _assignmentService.CreateAssignmentAsync(dto, adminId);
            return result.Execute();
        }

        [HttpPut("{id}/status")]
        public async Task<IResult> UpdateAssignmentStatus(int id, [FromBody] UpdateAssignmentStatusDto dto)
        {
            if (!ModelState.IsValid)
                return Results.BadRequest(ModelState);

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userId, out Guid parsedUserId))
                return Results.Unauthorized();

            var result = await _assignmentService.UpdateAssignmentStatusAsync(id, dto, parsedUserId);
            return result.Execute();
        }

        [HttpGet("request/{requestId}")]
        public async Task<IResult> GetAssignmentsByRequest(int requestId)
        {
            var result = await _assignmentService.GetAssignmentsByRequestAsync(requestId);
            return result.Execute();
        }

        [HttpGet("team/{reliefTeamId}")]
        public async Task<IResult> GetAssignmentsByReliefTeam(int reliefTeamId)
        {
            var result = await _assignmentService.GetAssignmentsByReliefTeamAsync(reliefTeamId);
            return result.Execute();
        }

        [HttpGet("{id}")]
        public async Task<IResult> GetAssignmentById(int id)
        {
            var result = await _assignmentService.GetAssignmentByIdAsync(id);
            return result.Execute();
        }

        [HttpGet]
        public async Task<IResult> GetAllAssignments()
        {
            var result = await _assignmentService.GetAllAssignmentsAsync();
            return result.Execute();
        }
    }
}

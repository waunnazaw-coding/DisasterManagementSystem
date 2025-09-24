using DisasterManagementSystem_Data.Models;
using DisasterManagementSystem_Data.Repositories;
using DisasterManagementSystem_Services.Models.AssistanceRequestDtos;
using DisasterManagementSystem_Services.Models.AssistanceRequestDtos.DisasterManagementSystem_Service.Models.Dtos;
using DisasterManagementSystem_Services.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DisasterManagementSystem_Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class AssistanceRequestsController : ControllerBase
    {
        private readonly IAssistanceRequestService _requestService;

        public AssistanceRequestsController(IAssistanceRequestService requestService)
        {
            _requestService = requestService;
        }

        [HttpPost]
        public async Task<IResult> CreateRequest([FromBody] CreateAssistanceRequestDto requestDto)
        {
            if (!ModelState.IsValid)
                return Results.BadRequest(ModelState);

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out Guid userId))
                return Results.Unauthorized();

            var result = await _requestService.CreateRequestAsync(requestDto, userId);
            return result.Execute();
        }

        [HttpGet]
        [Authorize(Roles = "Admin,SysAdmin,DisasterManagementAdmin")]
        public async Task<IResult> GetAllRequests([FromQuery] bool includeAssignments = false)
        {
            var result = includeAssignments
                ? await _requestService.GetAllRequestsWithAssignmentsAsync()
                : await _requestService.GetAllRequestsAsync();
            return result.Execute();
        }

        [HttpGet("my-requests")]
        public async Task<IResult> GetMyRequests()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out Guid userId))
                return Results.Unauthorized();

            var result = await _requestService.GetUserRequestsAsync(userId);
            return result.Execute();
        }
        [HttpGet("{id}")]
        public async Task<IResult> GetRequestById(int id)
        {
            // Always include assignments for the details view
            var result = await _requestService.GetRequestByIdAsync(id, true);
            return result.Execute();
        }

        [HttpPut("{id}")]
        public async Task<IResult> UpdateRequest(int id, [FromBody] UpdateAssistanceRequestDto requestDto)
        {
            if (!ModelState.IsValid)
                return Results.BadRequest(ModelState);

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out Guid userId))
                return Results.Unauthorized();

            // Ensure the DTO has the ID from the route
            if (requestDto == null) requestDto = new UpdateAssistanceRequestDto();

            var result = await _requestService.UpdateRequestAsync(id, requestDto, userId);
            return result.Execute();
        }

        [HttpDelete("{id}")]
        public async Task<IResult> DeleteRequest(int id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out Guid userId))
                return Results.Unauthorized();

            var result = await _requestService.DeleteRequestAsync(id, userId);
            return result.Execute();
        }

        [HttpPut("{id}/status")]
        [Authorize(Roles = "Admin,SysAdmin,DisasterManagementAdmin")]
        public async Task<IResult> UpdateRequestStatus(int id, [FromBody] UpdateRequestStatusDto statusDto)
        {
            if (!ModelState.IsValid)
                return Results.BadRequest(ModelState);

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out Guid userId))
                return Results.Unauthorized();

            var result = await _requestService.UpdateRequestStatusAsync(id, statusDto, userId);
            return result.Execute();
        }

        [HttpGet("disaster/{disasterEventId}")]
        public async Task<IResult> GetRequestsByDisaster(int disasterEventId)
        {
            var result = await _requestService.GetRequestsByDisasterAsync(disasterEventId);
            return result.Execute();
        }

        [HttpGet("status/{status}")]
        public async Task<IResult> GetRequestsByStatus(string status)
        {
            var result = await _requestService.GetRequestsByStatusAsync(status);
            return result.Execute();
        }
        // In AssistanceRequestsController.cs
        [HttpGet("stats")]
        [Authorize(Roles = "Admin,SysAdmin,DisasterManagementAdmin")]
        public async Task<IResult> GetRequestStats()
        {
            var result = await _requestService.GetRequestStatsAsync();
            return result.Execute();
        }

    }
}

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
        [Authorize(Roles = "Admin")]
        public async Task<IResult> GetAllRequests()
        {
            var result = await _requestService.GetAllRequestsAsync();
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
            var result = await _requestService.GetRequestByIdAsync(id);
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
        [Authorize(Roles = "Admin")]
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

    }
}

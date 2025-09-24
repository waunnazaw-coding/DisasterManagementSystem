using DisasterManagementSystem_Services.Models.ReliefTeamActivityDtos;
using DisasterManagementSystem_Services.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DisasterManagementSystem_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReliefTeamActivityController : ControllerBase
    {
        private readonly IReliefTeamActivityService _activityService;

        public ReliefTeamActivityController(IReliefTeamActivityService activityService)
        {
            _activityService = activityService;
        }

        [HttpPost]
        public async Task<IResult> CreateActivity([FromForm] CreateReliefTeamActivityDTO dto)
        {
            if (!ModelState.IsValid)
                return Results.BadRequest(ModelState);

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out Guid userId))
                return Results.Unauthorized();

            var result = await _activityService.CreateAsync(dto, userId);
            return result.Execute();
        }

        [HttpGet]
    
        public async Task<IResult> GetAllActivities()
        {
                var result = await _activityService.GetAllAsync();
            return result.Execute();
        }

        [HttpGet("my-activities")]
        public async Task<IResult> GetMyActivities()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out Guid userId))
                return Results.Unauthorized();

            var result = await _activityService.GetActivitiesByUserAsync(userId);
            return result.Execute();
        }

        [HttpGet("{id}")]
        public async Task<IResult> GetActivityById(int id)
        {
            var result = await _activityService.GetByIdAsync(id);
            return result.Execute();
        }

        // ReliefTeamActivityController.cs
        [HttpPut]
        public async Task<IResult> UpdateActivity([FromForm] UpdateReliefTeamActivityDTO dto) // Changed to [FromForm]
        {
            if (!ModelState.IsValid)
                return Results.BadRequest(ModelState);

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out Guid userId))
                return Results.Unauthorized();

            var result = await _activityService.UpdateAsync(dto, userId);
            return result.Execute();
        }

        [HttpDelete("{id}")]
        public async Task<IResult> DeleteActivity(int id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out Guid userId))
                return Results.Unauthorized();

            var result = await _activityService.DeleteAsync(id, userId);
            return result.Execute();
        }

        [HttpGet("team/{teamId}")]
        public async Task<IResult> GetActivitiesByTeam(int teamId)
        {
            var result = await _activityService.GetActivitiesByTeamAsync(teamId);
            return result.Execute();
        }

        [HttpGet("type/{activityType}")]
        public async Task<IResult> GetActivitiesByType(string activityType)
        {
            var result = await _activityService.GetActivitiesByTypeAsync(activityType);
            return result.Execute();
        }

        [HttpGet("stats")]
      //  [Authorize(Roles = "SysAdmin")]
        public async Task<IResult> GetActivityStats()
  {
            var result = await _activityService.GetActivityStatsAsync();
            return result.Execute();
        }
    }
}


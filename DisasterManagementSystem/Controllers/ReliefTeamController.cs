using DisasterManagementSystem_Services.Models;
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
    public class ReliefTeamController : ControllerBase
    {
        private readonly IReliefTeamService _reliefTeamService;

        public ReliefTeamController(IReliefTeamService reliefTeamService)
        {
            _reliefTeamService = reliefTeamService;
        }

        // ✅ POST: api/ReliefTeams
        [HttpPost]
        public async Task<IResult> CreateTeam([FromBody] CreateReliefTeamDto teamDto)
        {
            if (!ModelState.IsValid)
                return Results.BadRequest(ModelState);

            var result = await _reliefTeamService.CreateTeamAsync(teamDto);
            return result.Execute();
        }

        // ✅ GET: api/ReliefTeams
        [HttpGet]
        public async Task<IResult> GetAllTeams()
        {
            var result = await _reliefTeamService.GetAllTeamsAsync();
            return result.Execute();
        }

        // ✅ GET: api/ReliefTeams/{id}
        [HttpGet("{id}")]
        public async Task<IResult> GetTeamById(int id)
        {
           // var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _reliefTeamService.GetTeamByIdAsync(id);
            return result.Execute();
        }

        // ✅ PUT: api/ReliefTeams/{id}
        [HttpPut("{id}")]
        public async Task<IResult> UpdateTeam(int id, [FromBody] UpdateReliefTeamDto teamDto)
        {
            if (!ModelState.IsValid)
                return Results.BadRequest(ModelState);

            var result = await _reliefTeamService.UpdateTeamAsync(id, teamDto);
            return result.Execute();
        }

        // ✅ DELETE: api/ReliefTeams/{id}
        [HttpDelete("{id}")]
        public async Task<IResult> DeleteTeam(int id)
        {
            var result = await _reliefTeamService.DeleteTeamAsync(id);
            return result.Execute();
        }

        [HttpGet("by-user/{userId}")]
        public async Task<IResult> GetTeamByUserId(Guid userId)
        {
            var result = await _reliefTeamService.GetTeamByUserIdAsync(userId);
            return result.Execute();
        }

    }
}

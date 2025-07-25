using DisasterManagementSystem_Services.Models;
using DisasterManagementSystem_Services.Models.ReliefTeamDtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DisasterManagementSystem_Services.Services.Interfaces;

[ApiController]
[Route("api/[controller]")]
public class ReliefTeamController : ControllerBase
{
    private readonly IReliefTeamService _reliefTeamService;

    public ReliefTeamController(IReliefTeamService reliefTeamService)
    {
        _reliefTeamService = reliefTeamService;
    }

    [HttpPost("create-invite")]
    //[Authorize(Roles = "Admin,SysAdmin")]
    public async Task<IResult> CreateAndInvite([FromBody] CreateReliefTeamRequestDto dto)
    {
        if (!ModelState.IsValid)
            return Results.BadRequest(ModelState);

        var result = await _reliefTeamService.CreateReliefTeamAndInviteAsync(dto);

        // Call Execute() to return appropriate IActionResult or IResult with status code and body
        return result.Execute();
    }
}
using DisasterManagementSystem_Services.Models;
using DisasterManagementSystem_Services.Models.ReliefTeamDtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DisasterManagementSystem_Services.Services.Interfaces;

[ApiController]
[Route("api/reliefteams")]
public class ReliefTeamController : ControllerBase
{
    private readonly IReliefTeamsService _reliefTeamService;

    public ReliefTeamController(IReliefTeamsService reliefTeamService)
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
    
    [HttpGet]
    public async Task<IResult> GetAll()
    {
        var result = await _reliefTeamService.GetAllAsync();
        return result.Execute();
    }

    [HttpGet("{id:int}")]
    public async Task<IResult> GetById(int id)
    {
        var result = await _reliefTeamService.GetByIdAsync(id);
        return result.Execute();
    }

    [HttpPut("{id:int}")]
    public async Task<IResult> Update(int id, [FromBody] UpdateReliefTeamRequestDto dto)
    {
        if (!ModelState.IsValid)
            return Results.BadRequest(ModelState);

        var result = await _reliefTeamService.UpdateAsync(id, dto);
        return result.Execute();
    }

    [HttpDelete("{id:int}")]
    public async Task<IResult> Delete(int id)
    {
        var result = await _reliefTeamService.DeleteAsync(id);
        return result.Execute();
    }
}
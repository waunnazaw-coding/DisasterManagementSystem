using DisasterManagementSystem_Data.Repositories.Implements;
using DisasterManagementSystem_Data.Repositories.Interfaces;
using DisasterManagementSystem_Services.Models.UserDtos;
using DisasterManagementSystem_Services.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DisasterManagementSystem_Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,SysAdmin")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IUserRepository _userRepository; // Use interface instead of concrete class

        public UsersController(IUserService userService, IUserRepository userRepository)
        {
            _userService = userService;
            _userRepository = userRepository; // Initialize the repository
        }

        [HttpGet]
        public async Task<IResult> GetPaginated(
    [FromQuery] int pageNumber = 1,
    [FromQuery] int pageSize = 10,
    [FromQuery] string search = null,
    [FromQuery] string role = null,
    [FromQuery] string status = null)
        {
            var result = await _userService.GetPaginatedAsync(
                pageNumber,
                pageSize,
                search,
                role,
                status);
            return result.Execute();
        }
        [HttpGet("{id}")]
        public async Task<IResult> GetById(Guid id)
        {
            var result = await _userService.GetByIdAsync(id);
            return result.Execute();
        }

        [HttpPost]
        public async Task<IResult> Create([FromBody] UserCreateDto dto)
        {
            var result = await _userService.CreateAsync(dto);
            return result.Execute();
        }

        [HttpPut]
        public async Task<IResult> Update([FromBody] UserUpdateDto dto)
        {
            var result = await _userService.UpdateAsync(dto);
            return result.Execute();
        }

        [HttpDelete("{id}")]
        public async Task<IResult> Delete(Guid id)
        {
            var result = await _userService.DeleteAsync(id);
            return result.Execute();
        }

        [HttpPost("{id}/block")]
        public async Task<IResult> Block(Guid id)
        {
            var result = await _userService.BlockUserAsync(id);
            return result.Execute();
        }

        [HttpPost("{id}/unblock")]
        public async Task<IResult> Unblock(Guid id)
        {
            var result = await _userService.UnblockUserAsync(id);
            return result.Execute();
        }

        [HttpPost("{id}/changerole")]
        public async Task<IResult> ChangeRole(Guid id, [FromQuery] string role)
        {
            var result = await _userService.ChangeUserRoleAsync(id, role);
            return result.Execute();
        }
        [HttpGet("stats")]
        public async Task<IResult> GetStats()
            {
            var totalUsers = await _userRepository.CountAsync();
            var activeUsers = await _userRepository.CountAsync(status: "Active");
            var blockedUsers = await _userRepository.CountAsync(status: "Blacklisted");
            var admins = await _userRepository.CountAsync(role: "Admin");
            var sysAdmins = await _userRepository.CountAsync(role: "SysAdmin");
            var reliefTeams = await _userRepository.CountAsync(role: "ReliefTeam");

            var regularUsers = await _userRepository.CountAsync(role: "User");

            var stats = new
            {
                TotalUsers = totalUsers,
                ActiveUsers = activeUsers,
                BlockedUsers = blockedUsers,
                Admins = admins,
                SysAdmins = sysAdmins,
                ReliefTeams = reliefTeams,
                RegularUsers = regularUsers
            };

            return Results.Ok(stats);
        }
    }
}

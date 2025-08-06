using DisasterManagementSystem_Data.Models;
using DisasterManagementSystem_Services.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisasterManagementSystem_Services.Services.Interfaces
{
    public interface IReliefTeamService
    {
        Task<Result<ReliefTeamDto>> CreateTeamAsync(CreateReliefTeamDto dto);
        Task<Result<List<ReliefTeamDto>>> GetAllTeamsAsync();
        Task<Result<ReliefTeamDto>> GetTeamByIdAsync(int id);
        Task<Result<ReliefTeamDto>> UpdateTeamAsync(int id, UpdateReliefTeamDto dto);
        Task<Result<bool>> DeleteTeamAsync(int id);
        Task<Result<ReliefTeam>> GetTeamByUserIdAsync(Guid userId);
    }
}

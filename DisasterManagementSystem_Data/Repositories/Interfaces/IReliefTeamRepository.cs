using DisasterManagementSystem_Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisasterManagementSystem_Data.Repositories.Interfaces
{
    public interface IReliefTeamRepository
    {
        Task<ReliefTeam> GetByIdAsync(int id);
        Task<List<ReliefTeam>> GetAllAsync();
        Task<List<ReliefTeam>> GetByLocationAsync(int locationId);
        Task<ReliefTeam> CreateAsync(ReliefTeam team);
        Task<bool> UpdateAsync(ReliefTeam team);
        Task<bool> DeleteAsync(int id);

        Task<List<User>> GetTeamMembersAsync(int reliefTeamId);
        Task<bool> AddTeamMemberAsync(int reliefTeamId, Guid userId);
        Task<bool> IsUserInTeam(int reliefTeamId, Guid userId);// IReliefTeamRepository.cs - Add this method
        Task<ReliefTeam> GetByUserIdAsync(Guid userId);
        Task<List<ReliefTeam>> GetTeamsByUserIdAsync(Guid userId);
        Task<int?> GetReliefTeamIdByUserIdAsync(Guid userId);
        Task<Guid> GetUserIdByReliefTeamIdAsync(int teamId);
    }
}

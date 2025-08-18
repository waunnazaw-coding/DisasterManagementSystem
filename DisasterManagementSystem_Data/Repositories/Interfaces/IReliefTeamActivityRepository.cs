using DisasterManagementSystem_Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisasterManagementSystem_Data.Repositories.Interfaces
{
    public interface IReliefTeamActivityRepository
    {
        Task<ReliefTeamActivity> GetByIdAsync(int id, bool includeMedia = false, bool includeRelated = false);
        Task<List<ReliefTeamActivity>> GetAllAsync(bool includeMedia = false,bool includeRelated=false);
        Task<ReliefTeamActivity> AddAsync(ReliefTeamActivity entity);
        Task<ReliefTeamActivity> UpdateAsync(ReliefTeamActivity entity);
        Task<bool> DeleteAsync(ReliefTeamActivity entity);
        Task<List<ReliefTeamActivity>> GetByUserIdAsync(Guid userId, bool includeMedia = false);
        Task<List<ReliefTeamActivity>> GetByTeamIdAsync(int teamId, bool includeMedia = false);
        Task<List<ReliefTeamActivity>> GetByTypeAsync(string activityType, bool includeMedia = false);
        Task<int> GetCountAsync();
        Task<Dictionary<string, int>> GetCountByTypeAsync();
        Task<List<ReliefTeamActivity>> GetRecentAsync(int count);
    }
}

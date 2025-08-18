using DisasterManagementSystem_Services.Models;
using DisasterManagementSystem_Services.Models.ReliefTeamActivityDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisasterManagementSystem_Services.Services.Interfaces
{
    public interface IReliefTeamActivityService
    {
        Task<Result<ReliefTeamActivityDTO>> CreateAsync(CreateReliefTeamActivityDTO dto, Guid currentUserId);
        Task<Result<List<ReliefTeamActivityDTO>>> GetAllAsync();
        Task<Result<List<ReliefTeamActivityDTO>>> GetActivitiesByUserAsync(Guid userId);
        Task<Result<ReliefTeamActivityDTO>> GetByIdAsync(int id);
        Task<Result<ReliefTeamActivityDTO>> UpdateAsync(UpdateReliefTeamActivityDTO dto, Guid currentUserId);
        Task<Result<bool>> DeleteAsync(int id, Guid currentUserId);
        Task<Result<List<ReliefTeamActivityDTO>>> GetActivitiesByTeamAsync(int teamId);
        Task<Result<List<ReliefTeamActivityDTO>>> GetActivitiesByTypeAsync(string activityType);
        Task<Result<ActivityStatsDTO>> GetActivityStatsAsync();
    }
}

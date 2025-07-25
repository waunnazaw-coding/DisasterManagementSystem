using DisasterManagementSystem_Data.Models;

namespace DisasterManagementSystem_Data.Repositories.Interfaces;

public interface IUserReliefTeamRepository
{
    Task<UserReliefTeam?> FindAsync(Guid userId, int reliefTeamId);
    Task AddAsync(UserReliefTeam userReliefTeam);
    Task SaveChangesAsync();
}
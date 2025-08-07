using DisasterManagementSystem_Data.Models;

namespace DisasterManagementSystem_Data.Repositories.Interfaces;

public interface IReliefTeamRepository
{
    Task AddAsync(ReliefTeam entity);
    Task<IEnumerable<ReliefTeam>> GetAllAsync();
    Task<ReliefTeam?> GetByIdAsync(int id);
    Task UpdateAsync(ReliefTeam entity);
    Task DeleteAsync(ReliefTeam entity);
    Task SaveChangesAsync();
}
using DisasterManagementSystem_Data.Models;

namespace DisasterManagementSystem_Data.Repositories.Interfaces;

public interface IReliefTeamRepository
{
    Task<ReliefTeam?> GetByIdAsync(int id);
    Task<ReliefTeam?> GetByEmailAsync(string email);
    Task AddAsync(ReliefTeam team);
    void Update(ReliefTeam team);
    Task SaveChangesAsync();
}
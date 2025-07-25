using DisasterManagementSystem_Data.Models;
using DisasterManagementSystem_Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DisasterManagementSystem_Data.Repositories.Implements;

public class UserReliefTeamRepository : IUserReliefTeamRepository
{
    private readonly AppDbContext _context;

    public UserReliefTeamRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<UserReliefTeam?> FindAsync(Guid userId, int reliefTeamId)
    {
        return await _context.UserReliefTeams
            .FirstOrDefaultAsync(urt => urt.UserId == userId && urt.ReliefTeamId == reliefTeamId);
    }

    public async Task AddAsync(UserReliefTeam userReliefTeam)
    {
        await _context.UserReliefTeams.AddAsync(userReliefTeam);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}

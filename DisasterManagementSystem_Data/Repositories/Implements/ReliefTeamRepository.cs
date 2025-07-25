using DisasterManagementSystem_Data.Models;
using DisasterManagementSystem_Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DisasterManagementSystem_Data.Repositories.Implements;

public class ReliefTeamRepository  : IReliefTeamRepository
{
    
    private readonly AppDbContext _context;

    public ReliefTeamRepository(AppDbContext context)
    {
        _context = context;
    }
    
    public async Task<ReliefTeam?> GetByIdAsync(int id)
    {
        return await _context.ReliefTeams.FindAsync(id);
    }

    public async Task<ReliefTeam?> GetByEmailAsync(string email)
    {
       return await _context.ReliefTeams.FirstOrDefaultAsync(e => e.Email == email);
    }

    public async Task AddAsync(ReliefTeam team)
    {
        await _context.ReliefTeams.AddAsync(team);
    }

    public void Update(ReliefTeam team)
    {
        _context.ReliefTeams.Update(team);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
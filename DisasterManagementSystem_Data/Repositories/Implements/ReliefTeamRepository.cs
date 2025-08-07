using DisasterManagementSystem_Data.Models;
using DisasterManagementSystem_Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DisasterManagementSystem_Data.Repositories.Implements
{
    public class ReliefTeamRepository : IReliefTeamRepository
    {
        private readonly AppDbContext _context;

        public ReliefTeamRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(ReliefTeam entity)
        {
            await _context.ReliefTeams.AddAsync(entity);
        }

        public async Task<IEnumerable<ReliefTeam>> GetAllAsync()
        {
            return await _context.ReliefTeams.ToListAsync();
        }

        public async Task<ReliefTeam?> GetByIdAsync(int id)
        {
            return await _context.ReliefTeams.FindAsync(id);
        }

        public async Task UpdateAsync(ReliefTeam entity)
        {
            _context.ReliefTeams.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(ReliefTeam entity)
        {
            _context.ReliefTeams.Remove(entity);
            await _context.SaveChangesAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
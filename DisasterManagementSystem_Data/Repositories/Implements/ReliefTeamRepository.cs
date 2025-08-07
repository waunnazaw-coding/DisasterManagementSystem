using DisasterManagementSystem_Data.Models;
using DisasterManagementSystem_Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisasterManagementSystem_Data.Repositories.Implements
{

    public class ReliefTeamRepository : IReliefTeamRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ReliefTeamRepository> _logger;

        public ReliefTeamRepository(AppDbContext context, ILogger<ReliefTeamRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<ReliefTeam> GetByIdAsync(int id)
        {
            return await _context.ReliefTeams
                .Include(rt => rt.Location)
                .FirstOrDefaultAsync(rt => rt.Id == id);
        }

        public async Task<List<ReliefTeam>> GetAllAsync()
        {
            return await _context.ReliefTeams
                .Include(rt => rt.Location)
                .Where(rt => rt.Status == "Active")
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<ReliefTeam>> GetByLocationAsync(int locationId)
        {
            return await _context.ReliefTeams
                .Where(rt => rt.LocationId == locationId && rt.Status == "Active")
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<ReliefTeam> CreateAsync(ReliefTeam team)
        {
            await _context.ReliefTeams.AddAsync(team);
            await _context.SaveChangesAsync();
            return team;
        }

        public async Task<bool> UpdateAsync(ReliefTeam team)
        {
            _context.ReliefTeams.Update(team);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var team = await _context.ReliefTeams.FindAsync(id);
            if (team == null) return false;

            team.Status = "Inactive";
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<List<User>> GetTeamMembersAsync(int reliefTeamId)
        {
            return await _context.RequestAssignments
                .Where(ra => ra.ReliefTeamId == reliefTeamId && ra.AssignedBy != null)
                .Select(ra => ra.AssignedByNavigation)
                .Distinct()
                .ToListAsync();
        }

        public async Task<bool> IsUserInTeam(int reliefTeamId, Guid userId)
        {
            return await _context.RequestAssignments
                .AnyAsync(ra => ra.ReliefTeamId == reliefTeamId &&
                               ra.AssignedBy == userId);
        }

        public async Task<bool> AddTeamMemberAsync(int reliefTeamId, Guid userId)
        {
            // Create a dummy assignment to establish membership
            var assignment = new RequestAssignment
            {
                ReliefTeamId = reliefTeamId,
                AssignedBy = userId,
                AssistanceRequestId = -1, // Temporary value
                Status = "Membership",
                AssignedAt = DateTime.UtcNow
            };

            _context.RequestAssignments.Add(assignment);
            return await _context.SaveChangesAsync() > 0;
        }

        // ReliefTeamRepository.cs - Implement the method
        public async Task<ReliefTeam> GetByUserIdAsync(Guid userId)
        {
            return await _context.ReliefTeams
                .FirstOrDefaultAsync(rt => rt.UserId == userId);
        }
    }
}

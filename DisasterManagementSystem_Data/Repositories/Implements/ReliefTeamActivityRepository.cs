using DisasterManagementSystem_Data.Models;
using DisasterManagementSystem_Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisasterManagementSystem_Data.Repositories.Implements
{

    public class ReliefTeamActivityRepository : IReliefTeamActivityRepository
    {
        private readonly AppDbContext _context;

        public ReliefTeamActivityRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ReliefTeamActivity> AddAsync(ReliefTeamActivity entity)
        {
            _context.ReliefTeamActivities.Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<bool> DeleteAsync(ReliefTeamActivity entity)
        {
            _context.ReliefTeamActivities.Remove(entity);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<List<ReliefTeamActivity>> GetAllAsync(bool includeMedia = false, bool includeRelated = false)
        {
            var query = _context.ReliefTeamActivities.AsQueryable();

            if (includeRelated)
            {
                query = query
                    .Include(a => a.ReportPhotos)
                    .Include(a => a.ReliefTeam)  // Add this
                    .Include(a => a.PostedByNavigation);  // Add this
            }

            return await query.ToListAsync();
        }

        public async Task<ReliefTeamActivity> GetByIdAsync(int id, bool includeMedia = false,bool includeRelated=false)
        {
            var query = _context.ReliefTeamActivities.AsQueryable();

            if (includeRelated)
            {
                query = query
            .Include(a => a.ReportPhotos)
            .Include(a => a.ReliefTeam)  // Add this
            .Include(a => a.PostedByNavigation); // Add thi
            }

            return await query.FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<ReliefTeamActivity> UpdateAsync(ReliefTeamActivity entity)
        {
            _context.Entry(entity).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<List<ReliefTeamActivity>> GetByUserIdAsync(Guid userId, bool includeMedia = false)
        {
            var query = _context.ReliefTeamActivities
                .Where(a => a.PostedBy == userId);

            if (includeMedia)
            {
                query = query.Include(a => a.ReportPhotos);
            }

            return await query.ToListAsync();
        }

        public async Task<List<ReliefTeamActivity>> GetByTeamIdAsync(int teamId, bool includeMedia = false)
        {
            var query = _context.ReliefTeamActivities
                .Where(a => a.ReliefTeamId == teamId);

            if (includeMedia)
            {
                query = query.Include(a => a.ReportPhotos);
            }

            return await query
                .OrderByDescending(a => a.ActivityDate)
                .ToListAsync();
        }

        public async Task<List<ReliefTeamActivity>> GetByTypeAsync(string activityType, bool includeMedia = false)
        {
            var query = _context.ReliefTeamActivities
                .Where(a => a.ActivityType == activityType);

            if (includeMedia)
            {
                query = query.Include(a => a.ReportPhotos);
            }

            return await query.ToListAsync();
        }

        public async Task<int> GetCountAsync()
        {
            return await _context.ReliefTeamActivities.CountAsync();
        }

        public async Task<Dictionary<string, int>> GetCountByTypeAsync()
        {
            return await _context.ReliefTeamActivities
                .GroupBy(a => a.ActivityType)
                .Select(g => new { Type = g.Key, Count = g.Count() })
                .ToDictionaryAsync(g => g.Type, g => g.Count);
        }

        public async Task<List<ReliefTeamActivity>> GetRecentAsync(int count)
        {
            return await _context.ReliefTeamActivities
                .OrderByDescending(a => a.CreatedAt)
                .Take(count)
                .Include(a => a.ReportPhotos)
                .ToListAsync();
        }
    }
}

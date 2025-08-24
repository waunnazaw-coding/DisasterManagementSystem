using DisasterManagementSystem_Data.Models;
using DisasterManagementSystem_Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DisasterManagementSystem_Data.Repositories.Implements
{
    public class RequestAssignmentRepository : IRequestAssignmentRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<RequestAssignmentRepository> _logger;

        public RequestAssignmentRepository(
            AppDbContext context,
            ILogger<RequestAssignmentRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<RequestAssignment> GetByIdAsync(int id)
        {
            return await _context.RequestAssignments
                .Include(ra => ra.AssistanceRequest)
                .Include(ra => ra.ReliefTeam)
                .Include(ra => ra.AssignedByNavigation) // ✅ fixed
                .FirstOrDefaultAsync(ra => ra.Id == id);
        }

        public async Task<IEnumerable<RequestAssignment>> GetAllAsync()
        {
            return await _context.RequestAssignments
                .Include(ra => ra.AssistanceRequest)
                .Include(ra => ra.ReliefTeam)
                .Include(ra => ra.AssignedByNavigation) // ✅ fixed
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<RequestAssignment>> GetByRequestIdAsync(int requestId)
        {
            return await _context.RequestAssignments
                .Where(ra => ra.AssistanceRequestId == requestId)
                .Include(ra => ra.ReliefTeam)
                .Include(ra => ra.AssignedByNavigation) // ✅ fixed
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<RequestAssignment>> GetByReliefTeamIdAsync(int reliefTeamId)
        {
            return await _context.RequestAssignments
                .Where(ra => ra.ReliefTeamId == reliefTeamId)
                .Include(ra => ra.AssistanceRequest)
                .Include(ra => ra.AssignedByNavigation)
                .OrderByDescending(ra => ra.AssignedAt) // 👈 Sort by latest date
                .AsNoTracking()
                .ToListAsync();
        }


        public async Task<RequestAssignment> CreateAsync(RequestAssignment assignment)
        {
            await _context.RequestAssignments.AddAsync(assignment);
            await _context.SaveChangesAsync();
            return assignment;
        }

        public async Task<bool> UpdateAsync(RequestAssignment assignment)
        {
            _context.RequestAssignments.Update(assignment);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var assignment = await _context.RequestAssignments.FindAsync(id);
            if (assignment == null) return false;

            _context.RequestAssignments.Remove(assignment);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateStatusAsync(int id, string status, Guid updatedBy)
        {
            var assignment = await _context.RequestAssignments.FindAsync(id);
            if (assignment == null) return false;

            assignment.Status = status;
            assignment.LastUpdatedBy = updatedBy;
            assignment.UpdatedAt = DateTime.UtcNow;

            if (status == "Done")
            {
                assignment.CompletedAt = DateTime.UtcNow;
            }

            return await _context.SaveChangesAsync() > 0;
        }
    }
}

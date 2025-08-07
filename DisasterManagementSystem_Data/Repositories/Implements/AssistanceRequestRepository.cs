
using DisasterManagementSystem_Data.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DisasterManagementSystem_Data.Repositories
{
    public class AssistanceRequestRepository : IAssistanceRequestRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<AssistanceRequestRepository> _logger;

        public AssistanceRequestRepository(AppDbContext context,ILogger<AssistanceRequestRepository>logger)
        {
            _context = context;
            _logger = logger;
        }



        public async Task<AssistanceRequest> AddAsync(AssistanceRequest request)
        {
            try
            {
                // Ensure null values for optional relationships
                if (request.DisasterReportId.HasValue && request.DisasterReportId.Value == 0)
                    request.DisasterReportId = null;

                if (request.DisasterEventId.HasValue && request.DisasterEventId.Value == 0)
                    request.DisasterEventId = null;

                if (request.LocationId.HasValue && request.LocationId.Value == 0)
                    request.LocationId = null;

                await _context.AssistanceRequests.AddAsync(request);
                await _context.SaveChangesAsync();
                return request;
            }
            catch (DbUpdateException dbEx)
            {
                if (dbEx.InnerException is SqlException sqlEx && sqlEx.Number == 547)
                {
                    // Foreign key violation
                    _logger.LogError("Foreign key violation: {Message}", sqlEx.Message);
                    throw new InvalidOperationException("Invalid reference to related entity", sqlEx);
                }
                throw;
            }
        }
        public async Task<IEnumerable<AssistanceRequest>> GetAllAsync()
        {
            return await _context.AssistanceRequests
                .Include(r => r.DisasterEvent)
                .Include(r => r.User)
                .Include(r => r.Location)
                .Include(r => r.DisasterReport)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<AssistanceRequest> GetByIdAsync(int id)
        {
            return await _context.AssistanceRequests
                .Include(r => r.DisasterEvent)
                .Include(r => r.User)
                .Include(r => r.Location)
                .Include(r => r.DisasterReport)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<bool> UpdateAsync(AssistanceRequest request)
        {
            try
            {
                // Convert 0 to null for optional relationships
                request.LocationId = request.LocationId == 0 ? null : request.LocationId;
                request.DisasterEventId = request.DisasterEventId == 0 ? null : request.DisasterEventId;

                _context.AssistanceRequests.Update(request);
                return await _context.SaveChangesAsync() > 0;
            }
            catch (DbUpdateException dbEx)
            {
                if (dbEx.InnerException is SqlException sqlEx && sqlEx.Number == 547)
                {
                    throw new InvalidOperationException("Foreign key violation", dbEx);
                }
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var request = await _context.AssistanceRequests.FindAsync(id);
            if (request == null) return false;

            _context.AssistanceRequests.Remove(request);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<IEnumerable<AssistanceRequest>> GetByUserIdAsync(Guid userId)
        {
            return await _context.AssistanceRequests
                .Where(r => r.UserId == userId)
                .Include(r => r.DisasterEvent)
                .Include(r => r.Location)
                .Include(r => r.DisasterReport)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<AssistanceRequest>> GetByDisasterEventAsync(int disasterEventId)
        {
            return await _context.AssistanceRequests
                .Where(r => r.DisasterEventId == disasterEventId)
                .Include(r => r.User)
                .Include(r => r.Location)
                .Include(r => r.DisasterReport)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<AssistanceRequest>> GetByStatusAsync(string status)
        {
            return await _context.AssistanceRequests
                .Where(r => r.Status == status)
                .Include(r => r.DisasterEvent)
                .Include(r => r.User)
                .Include(r => r.Location)
                .Include(r => r.DisasterReport)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task LoadRelatedEntitiesAsync(AssistanceRequest request)
        {
            if (request.DisasterEventId.HasValue)
            {
                await _context.Entry(request)
                    .Reference(r => r.DisasterEvent)
                    .LoadAsync();
            }

            if (request.LocationId.HasValue)
            {
                await _context.Entry(request)
                    .Reference(r => r.Location)
                    .LoadAsync();
            }
        }

        public async Task<DisasterEvent?> GetDisasterEventAsync(int? disasterEventId)
        {
            if (!disasterEventId.HasValue) return null;
            return await _context.DisasterEvents.FindAsync(disasterEventId.Value);
        }
        // Add this method to your AssistanceRequestRepository
        public async Task<IEnumerable<AssistanceRequest>> GetAllWithAssignmentsAsync()
        {
            return await _context.AssistanceRequests
                .Include(r => r.DisasterEvent)
                .Include(r => r.User)
                .Include(r => r.Location)
                .Include(r => r.DisasterReport)
                .Include(r => r.RequestAssignments)
                    .ThenInclude(a => a.AssignedByNavigation) // This is the correct navigation property
                .Include(r => r.RequestAssignments)
                    .ThenInclude(a => a.ReliefTeam)
                .AsNoTracking()
                .ToListAsync();
        }
    }
}


using DisasterManagementSystem_Data.Models;
using Microsoft.EntityFrameworkCore;

namespace DisasterManagementSystem_Data.Repositories
{
    public class AssistanceRequestRepository : IAssistanceRequestRepository
    {
        private readonly AppDbContext _context;

        public AssistanceRequestRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<AssistanceRequest>> GetAllAsync()
        {
            return await _context.AssistanceRequests
                .Include(a => a.DisasterEvent)
                .Include(a => a.DisasterReport)
                .Include(a => a.Location)
                .Include(a => a.User)
                .ToListAsync();
        }

        public async Task<AssistanceRequest?> GetByIdAsync(int id)
        {
            return await _context.AssistanceRequests
                .Include(a => a.DisasterEvent)
                .Include(a => a.DisasterReport)
                .Include(a => a.Location)
                .Include(a => a.User)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task AddAsync(AssistanceRequest request)
        {
            await _context.AssistanceRequests.AddAsync(request);
        }

        public async Task UpdateAsync(AssistanceRequest request)
        {
            _context.AssistanceRequests.Update(request);
        }

        public async Task DeleteAsync(int id)
        {
            var request = await GetByIdAsync(id);
            if (request != null)
            {
                _context.AssistanceRequests.Remove(request);
            }
        }

        public async Task<bool> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }
    }
}

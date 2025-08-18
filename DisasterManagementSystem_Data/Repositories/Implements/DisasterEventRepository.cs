using DisasterManagementSystem_Data.Models;
using Microsoft.EntityFrameworkCore;

namespace DisasterManagementSystem_Data.Repositories
{
    public class DisasterEventRepository : IDisasterEventRepository
    {
        private readonly AppDbContext _context;

        public DisasterEventRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.DisasterReports.AnyAsync(r => r.Id == id);
        }
        public async Task<DisasterEvent?> GetByIdAsync(int id) =>
            await _context.DisasterEvents
                .Include(e => e.DisasterType)
                .Include(e => e.Location)
                .Include(e => e.Impacts)
                .Include(e => e.ReportPhotos)
                .Include(e => e.CreatedUser)
                .Include(e => e.UpdatedUser)
                .FirstOrDefaultAsync(e => e.Id == id);

        public async Task<IEnumerable<DisasterEvent>> GetAllAsync() =>
            await _context.DisasterEvents
                .Include(e => e.DisasterType) // Include DisasterType
                .Include(e => e.Location)     // Include Location
                .ToListAsync();

        public async Task<int> CountVerifiedAsync()
        {
            return await _context.DisasterEvents
                .CountAsync(e => e.Status == "Active");
        }

        public async Task<IEnumerable<DisasterEvent>> GetAllActive() =>
            await _context.DisasterEvents
                .Include(e => e.DisasterType)
                .Include(e => e.Location)
                .Where(e => e.Status == "Active")
                .ToListAsync();

        public async Task<IEnumerable<DisasterEvent>> SearchByNameAsync(string name)
        {
            return await _context.DisasterEvents
                .Include(e => e.DisasterType)
                .Include(e => e.Location)
                .Where(e => e.Name.Contains(name))
                .ToListAsync();
        }


        public async Task AddAsync(DisasterEvent disasterEvent)
        {
            await _context.DisasterEvents.AddAsync(disasterEvent);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(DisasterEvent disasterEvent)
        {
            _context.DisasterEvents.Update(disasterEvent);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var disasterEvent = await GetByIdAsync(id);
            if (disasterEvent != null)
            {
                _context.DisasterEvents.Remove(disasterEvent);
                await _context.SaveChangesAsync();
            }
            else
            {
                throw new KeyNotFoundException($"DisasterEvent with ID {id} not found.");
            }
        }
    }
}

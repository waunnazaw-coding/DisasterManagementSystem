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

        public async Task<DisasterEvent?> GetByIdAsync(int id) =>
           await _context.DisasterEvents.FindAsync(id);

        public async Task<IEnumerable<DisasterEvent>> GetAllAsync() =>
            await _context.DisasterEvents.ToListAsync();

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
using DisasterManagementSystem_Data;
using DisasterManagementSystem_Data.Models;
using Microsoft.EntityFrameworkCore;

public class ImpactRepository : IImpactRepository
{
    private readonly AppDbContext _context;

    public ImpactRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddRangeAsync(IEnumerable<Impact> impacts)
    {
        await _context.Impacts.AddRangeAsync(impacts);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Impact>> GetAllAsync()
    {
        return await _context.Impacts.AsNoTracking().ToListAsync();
    }

    public async Task<IEnumerable<Impact>> GetByDisasterEventIdAsync(int disasterEventId)
    {
        return await _context.Impacts
            .AsNoTracking()
            .Where(i => i.DisasterEventId == disasterEventId)
            .ToListAsync();
    }

    public async Task<Impact?> GetByIdAsync(int id)
    {
        return await _context.Impacts.AsNoTracking().FirstOrDefaultAsync(i => i.Id == id);
    }
}

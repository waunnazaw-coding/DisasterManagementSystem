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
        // No SaveChanges here — caller will commit
    }

    public async Task<IEnumerable<Impact>> GetAllAsync()
    {
        return await _context.Impacts
            .AsNoTracking()
            .Include(i => i.DisasterEvent)
            .Include(i => i.DisasterReport)
            .ToListAsync();
    }

    public async Task<IEnumerable<Impact>> GetByDisasterEventIdAsync(int disasterEventId)
    {
        return await _context.Impacts
            .AsNoTracking()
            .Include(i => i.DisasterEvent)
            .Where(i => i.DisasterEventId == disasterEventId)
            .ToListAsync();
    }

    public async Task<Impact?> GetByIdAsync(int id)
    {
        return await _context.Impacts
            .Include(i => i.DisasterEvent)
            .FirstOrDefaultAsync(i => i.Id == id);
    }

    public async Task UpdateAsync(Impact impact)
    {
        var trackedEntity = await _context.Impacts.FindAsync(impact.Id);
        if (trackedEntity == null)
        {
            throw new Exception("Impact not found");
        }

        trackedEntity.Type = impact.Type;
        trackedEntity.Value = impact.Value;
        trackedEntity.ObjectName = impact.ObjectName;

        // EF will track changes — no SaveChanges here
    }

    public async Task DeleteAsync(Impact impact)
    {
        _context.Impacts.Remove(impact);
    }
}

using DisasterManagementSystem_Data.Models;
using Microsoft.EntityFrameworkCore;

public class DisasterTypeRepository : IDisasterTypeRepository
{
    private readonly AppDbContext _context;

    public DisasterTypeRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<DisasterType?> GetByIdAsync(int id)
    {
        return await _context.DisasterTypes.FindAsync(id);
    }

    public async Task<IEnumerable<DisasterType>> GetAllAsync()
    {
        return await _context.DisasterTypes.ToListAsync();
    }

    public async Task AddAsync(DisasterType disasterType)
    {
        await _context.DisasterTypes.AddAsync(disasterType);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(DisasterType disasterType)
    {
        _context.DisasterTypes.Update(disasterType);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var disasterType = await GetByIdAsync(id);
        if (disasterType != null)
        {
            _context.DisasterTypes.Remove(disasterType);
            await _context.SaveChangesAsync();
        }
        else
        {
            throw new KeyNotFoundException($"DisasterType with ID {id} not found.");
        }
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
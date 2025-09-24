using DisasterManagementSystem_Data.Models;
using Microsoft.EntityFrameworkCore;

public class LocationRepository : IlocationRepository
{
    private readonly AppDbContext _context;

    public LocationRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Location?> GetByIdAsync(int id) =>
        await _context.Locations.FindAsync(id);

    public async Task<IEnumerable<Location>> GetAllAsync() =>
        await _context.Locations.ToListAsync();

    public Task AddAsync(Location disasterArea)
    {
        _context.Locations.AddAsync(disasterArea);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Location location)
    {
        _context.Entry(location).State = EntityState.Modified;
        return Task.CompletedTask;
    }
    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.DisasterReports.AnyAsync(r => r.Id == id);
    }

    public async Task DeleteAsync(int locationId)
    {
        var location = await _context.Locations.FindAsync(locationId);
        if (location != null)
        {
            _context.Locations.Remove(location);
            await _context.SaveChangesAsync();
        }
    }

    public async Task SaveChangesAsync() =>
        await _context.SaveChangesAsync();
}

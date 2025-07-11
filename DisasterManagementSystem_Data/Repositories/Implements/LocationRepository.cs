using DisasterManagementSystem_Data.Models;
using Microsoft.EntityFrameworkCore;

public class LocatioinRepository : IlocationRepository
{
    private readonly AppDbContext _context;

    public LocatioinRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Location?> GetByIdAsync(int id) =>
        await _context.Locations.FindAsync(id);

    public async Task<IEnumerable<Location>> GetAllAsync() =>
        await _context.Locations.ToListAsync();

    public async Task AddAsync(Location disasterArea)
    {
        await _context.Locations.AddAsync(disasterArea);
    }

    public Task UpdateAsync(Location location)
    {
        _context.Entry(location).State = EntityState.Modified;
        return Task.CompletedTask;
    }


    public async Task DeleteAsync(int id)
    {
        var entity = await GetByIdAsync(id);
        if (entity != null)
            _context.Locations.Remove(entity);
    }

    public async Task SaveChangesAsync() =>
        await _context.SaveChangesAsync();
}

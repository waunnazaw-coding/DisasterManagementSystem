using DisasterManagementSystem_Data.Models;
using Microsoft.EntityFrameworkCore;

public class DisasterReportRepository : IDisasterReportRepository
{
    private readonly AppDbContext _context;

    public DisasterReportRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<DisasterReport?> GetByIdAsync(int id) =>
        await _context.DisasterReports.FindAsync(id);

    public async Task<IEnumerable<DisasterReport>> GetAllAsync() =>
        await _context.DisasterReports.ToListAsync();

    public Task AddAsync(DisasterReport report)
    {
        _context.DisasterReports.AddAsync(report);
        return Task.CompletedTask;
    }

    public async Task UpdateAsync(DisasterReport report)
    {
        _context.DisasterReports.Update(report);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var report = await GetByIdAsync(id);
        if (report != null)
        {
            _context.DisasterReports.Remove(report);
            await _context.SaveChangesAsync();
        }
        else
        {
            throw new KeyNotFoundException($"DisasterType with ID {id} not found.");
        }
    }
}

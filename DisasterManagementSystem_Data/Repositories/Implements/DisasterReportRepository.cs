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
        await _context.DisasterReports
        .Include(d => d.Location)
        .Include(d => d.ReportPhotos)
        .FirstOrDefaultAsync(d => d.Id == id);

    public async Task<IEnumerable<DisasterReport>> GetAllAsync() =>
        await _context.DisasterReports
            .Include(d => d.Location)
            .Include(d => d.ReportPhotos)
            .ToListAsync();

    public async Task<IEnumerable<DisasterReport>> GetAllConfirmedAsync() =>
        await _context.DisasterReports
            .Include(d => d.Location)
            .Include(d => d.ReportPhotos)
            .Where(d => d.Status == "Confirmed")
            .ToListAsync();

    public async Task AddAsync(DisasterReport report)
    {
        await _context.DisasterReports.AddAsync(report);
        await _context.SaveChangesAsync();
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
    // In DisasterReportRepository.cs
    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.DisasterReports.AnyAsync(r => r.Id == id);
    }
}

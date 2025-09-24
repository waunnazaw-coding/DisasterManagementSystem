using System.Runtime.Versioning;
using DisasterManagementSystem_Data;
using DisasterManagementSystem_Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class DeletedReportLogsController : ControllerBase
{
    private readonly AppDbContext _context;

    public DeletedReportLogsController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/DeletedReportLogs/deletion-reminders
    [HttpGet("deletion-reminders")]
    public async Task<IActionResult> GetDeletionReminders()
    {
        var now = DateTime.UtcNow;

        // 3 days before a month
        var targetDate = now.AddMonths(-1).AddDays(-3);

        // 1 month ago
        var oneMonthAgo = now.AddMonths(-1);

        // Fetch reports with status "Rejected" and updated date in the range
        var willDelete = await _context.DisasterReports
      .Where(r => r.Status == "Rejected" &&
                  r.UpdatedAt.HasValue &&
                  r.UpdatedAt.Value >= targetDate &&
                  r.UpdatedAt.Value < oneMonthAgo)
      .OrderBy(r => r.UpdatedAt)
      .Select(r => new
      {
          r.Id,
          r.Title,
          WillDeleteAt = r.UpdatedAt.Value.AddDays(3) // Now safe
      })
      .ToListAsync();

        // Fetch recently deleted logs (last 3 days)
        var recentlyDeleted = await _context.DeletedReportLogs
            .Where(l => l.DeletedAt >= now.AddDays(-3))
            .OrderByDescending(l => l.DeletedAt)
            .Select(l => new
            {
                l.Id,
                l.ReportName,
                l.DeletedAt,
                l.Status
            })
            .ToListAsync();

        return Ok(new
        {
            willDelete,
            recentlyDeleted
        });
    }

    // DELETE: api/DeletedReportLogs/{id} - delete log for a deleted report
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteLog(int id)
    {
        var log = await _context.DeletedReportLogs.FindAsync(id);
        if (log == null) return NotFound();

        _context.DeletedReportLogs.Remove(log);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/DeletedReportLogs/clear-old?days=30
    [HttpDelete("clear-old")]
    public async Task<IActionResult> ClearOldLogs([FromQuery] int days = 30)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-days);
        var oldLogs = await _context.DeletedReportLogs
            .Where(l => l.DeletedAt <= cutoffDate)
            .ToListAsync();

        if (!oldLogs.Any())
            return Ok("No old logs to delete.");

        _context.DeletedReportLogs.RemoveRange(oldLogs);
        await _context.SaveChangesAsync();

        return Ok($"Deleted {oldLogs.Count} old logs older than {days} days.");
    }
}

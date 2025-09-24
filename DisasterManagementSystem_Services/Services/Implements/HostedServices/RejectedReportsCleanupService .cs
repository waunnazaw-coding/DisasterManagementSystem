using DisasterManagementSystem_Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using DisasterManagementSystem_Data.Models;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

public class RejectedReportsCleanupService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;

    public RejectedReportsCleanupService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await CleanupRejectedReports(stoppingToken);

            // Run daily
            await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
        }
    }

    private async Task CleanupRejectedReports(CancellationToken stoppingToken)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var cutoffDate = DateTime.UtcNow.AddMonths(-1);

            var oldRejectedReports = await context.DisasterReports
                .Include(r => r.ReportPhotos)
                .Include(r => r.Impacts)
                .Where(r => r.Status == "Rejected" && r.UpdatedAt <= cutoffDate)
                .ToListAsync(stoppingToken);

            if (oldRejectedReports.Any())
            {
                foreach (var report in oldRejectedReports)
                {
                    // Collect extra info before deletion
                    var extraInfo = new
                    {
                        PhotosCount = report.ReportPhotos.Count,
                        ImpactsCount = report.Impacts.Count
                    };

                    var deletedLog = new DeletedReportLog
                    {
                        ReportId = report.Id,
                        ReportName = report.Title,
                        Status = report.Status,
                        DeletedAt = DateTime.UtcNow,
                        DeletedBy = "System",
                        ExtraInfo = JsonSerializer.Serialize(extraInfo)
                    };

                    await context.DeletedReportLogs.AddAsync(deletedLog, stoppingToken);

                    // Remove related entities
                    if (report.ReportPhotos.Any())
                        context.ReportPhotos.RemoveRange(report.ReportPhotos);

                    if (report.Impacts.Any())
                        context.Impacts.RemoveRange(report.Impacts);

                    // Remove the report itself
                    context.DisasterReports.Remove(report);
                }

                await context.SaveChangesAsync(stoppingToken);
                Console.WriteLine($"Deleted {oldRejectedReports.Count} rejected reports older than {cutoffDate} and logged them.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error cleaning rejected reports: {ex.Message}");
        }
    }
}

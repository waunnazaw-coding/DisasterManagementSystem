using DisasterManagementSystem_Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using DisasterManagementSystem_Data.Models;
using Microsoft.Extensions.DependencyInjection;

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

            // Wait 24 hours
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

            // Include ReportPhotos to delete them first
            var oldRejectedReports = await context.DisasterReports
                .Include(r => r.ReportPhotos) // Ensure you have a navigation property ReportPhotos
                .Include(r => r.Impacts)
                .Where(r => r.Status == "Rejected" && r.UpdatedAt <= cutoffDate)
                .ToListAsync(stoppingToken);

            if (oldRejectedReports.Any())
            {
                foreach (var report in oldRejectedReports)
                {
                    // Remove related photos first
                    if (report.ReportPhotos.Any())
                    {
                        context.ReportPhotos.RemoveRange(report.ReportPhotos);
                    }

                    if (report.Impacts.Any())
                        context.Impacts.RemoveRange(report.Impacts);
                }

                // Remove the reports
                context.DisasterReports.RemoveRange(oldRejectedReports);

                await context.SaveChangesAsync(stoppingToken);
                Console.WriteLine($"Deleted {oldRejectedReports.Count} rejected reports (and their photos) older than {cutoffDate}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error cleaning rejected reports: {ex.Message}");
        }
    }
}

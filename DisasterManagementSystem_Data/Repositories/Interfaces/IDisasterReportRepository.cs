using DisasterManagementSystem_Data.Models;

public interface IDisasterReportRepository
{
    Task<DisasterReport?> GetByIdAsync(int id);
    Task<DisasterReport?> GetByIdWithImpactAsync(int id);
    Task<IEnumerable<DisasterReport>> GetAllAsync();
    Task<IEnumerable<DisasterReport>> GetAllConfirmedAsync();
    Task AddAsync(DisasterReport report);
    Task UpdateAsync(DisasterReport report);
    Task DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
}

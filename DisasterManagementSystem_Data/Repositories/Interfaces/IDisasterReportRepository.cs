using DisasterManagementSystem_Data.Models;

public interface IDisasterReportRepository
{
    Task<DisasterReport?> GetByIdAsync(int id);
    Task<IEnumerable<DisasterReport>> GetAllAsync();
    Task AddAsync(DisasterReport report);
    Task UpdateAsync(DisasterReport report);
    Task DeleteAsync(int id);
}

using DisasterManagementSystem_Data.Models;

public interface IImpactRepository
{
    Task AddRangeAsync(IEnumerable<Impact> impacts);
    Task<IEnumerable<Impact>> GetAllAsync();
    Task<IEnumerable<Impact>> GetByDisasterEventIdAsync(int disasterEventId);
    Task<Impact?> GetByIdAsync(int id);
    Task UpdateAsync(Impact impact);
    Task DeleteAsync(Impact impact);
}

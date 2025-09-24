using DisasterManagementSystem_Data.Models;

public interface IDisasterTypeRepository
{
    Task<DisasterType> GetByIdAsync(int id);
    Task<IEnumerable<DisasterType?>> GetAllAsync();
    Task AddAsync(DisasterType disasterType);
    Task UpdateAsync(DisasterType disasterType);
    Task DeleteAsync(int id);
    Task SaveChangesAsync();
}
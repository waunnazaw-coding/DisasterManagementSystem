using DisasterManagementSystem_Data.Models;

public interface IlocationRepository
{
    Task<Location?> GetByIdAsync(int id);
    Task<IEnumerable<Location>> GetAllAsync();
    Task AddAsync(Location location);
    Task UpdateAsync(Location location);
    Task DeleteAsync(int id);
    Task SaveChangesAsync();
}

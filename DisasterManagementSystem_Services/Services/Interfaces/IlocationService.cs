using DisasterManagementSystem_Data.Models;
using DisasterManagementSystem_Services.Models.LocationDtos;

public interface IlocationService
{
    Task<Location?> GetByIdAsync(int id);
    Task<IEnumerable<Location>> GetAllAsync();
    Task AddAsync(LocationCreateDto dto);
    Task UpdateAsync(Location disasterArea);
    Task DeleteAsync(int id);
}

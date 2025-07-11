using DisasterManagementSystem_Data.Models;
using DisasterManagementSystem_Services.Models;
using DisasterManagementSystem_Services.Models.LocationDtos;
using AppLocation = DisasterManagementSystem_Data.Models.Location;

public interface IlocationService
{
    Task<Result<AppLocation>> GetByIdAsync(int id);
    Task<Result<IEnumerable<AppLocation>>> GetAllAsync();
    Task<Result<AppLocation>> AddAsync(LocationCreateDto dto);
    Task<Result<AppLocation>> UpdateAsync(AppLocation location);
    Task<Result<bool>> DeleteAsync(int id);
}

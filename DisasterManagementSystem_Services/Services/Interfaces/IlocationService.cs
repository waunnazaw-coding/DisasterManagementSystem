using DisasterManagementSystem_Data.Models;
using DisasterManagementSystem_Services.Models;
using DisasterManagementSystem_Services.Models.LocationDtos;
using AppLocation = DisasterManagementSystem_Data.Models.Location;

public interface IlocationService
{
    Task<Result<LocationDto>> GetByIdAsync(int id);
    Task<Result<IEnumerable<LocationDto>>> GetAllAsync();
    Task<Result<LocationDto>> AddAsync(LocationCreateDto dto);
    Task<Result<LocationDto>> PureAddAsync(LocationCreateDto dto);
    Task<Result<AppLocation>> UpdateAsync(LocationUpdateDto location);
    Task<Result<bool>> DeleteAsync(int id);
}

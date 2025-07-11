using DisasterManagementSystem_Data.Models;
using DisasterManagementSystem_Services.Models;
using DisasterManagementSystem_Services.Models.DisasterTypsDtos;

public interface IDisasterTypeService
{
    Task<Result<DisasterType>> GetByIdAsync(int id);
    Task<Result<IEnumerable<DisasterType>>> GetAllAsync();
    Task<Result<DisasterType>> AddAsync(DisasterTypeCreateDto dto);
    Task<Result<DisasterType>> UpdateAsync(DisasterTypeUpdateDto dto);
    Task<Result<bool>> DeleteAsync(int id);
}
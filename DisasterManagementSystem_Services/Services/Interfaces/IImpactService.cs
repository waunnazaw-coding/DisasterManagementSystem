using DisasterManagementSystem_Services.Models;

public interface IImpactService
{
    Task<bool> CreateImpactsAsync(IEnumerable<ImpactCreateDto> dtos, bool saveImmediately = true);
    Task<IEnumerable<ImpactDto>> GetAllAsync();
    Task<IEnumerable<ImpactDto>> GetByDisasterEventAsync(int disasterEventId);
    Task<ImpactDto?> GetByIdAsync(int id);
    Task<Result<bool>> UpdateImpactAsync(int id, ImpactUpdateDto dto, bool saveImmediately = true);
    Task<Result<bool>> DeleteImpactAsync(int id, bool saveImmediately = true);
}

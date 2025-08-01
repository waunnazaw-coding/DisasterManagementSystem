public interface IImpactService
{
    Task<bool> CreateImpactsAsync(IEnumerable<ImpactCreateDto> dtos);
    Task<IEnumerable<ImpactDto>> GetAllAsync();
    Task<IEnumerable<ImpactDto>> GetByDisasterEventAsync(int disasterEventId);
    Task<ImpactDto?> GetByIdAsync(int id);
}

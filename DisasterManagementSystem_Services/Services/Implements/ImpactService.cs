using DisasterManagementSystem_Data.Models;

public class ImpactService : IImpactService
{
    private readonly IImpactRepository _repository;

    public ImpactService(IImpactRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> CreateImpactsAsync(IEnumerable<ImpactCreateDto> dtos)
    {
        var impacts = dtos.Select(dto => new Impact
        {
            DisasterEventId = dto.DisasterEventId,
            Type = dto.Type,
            Value = dto.Value,
            ObjectName = dto.ObjectName
        });

        await _repository.AddRangeAsync(impacts);
        return true;
    }

    public async Task<IEnumerable<ImpactDto>> GetAllAsync()
    {
        var impacts = await _repository.GetAllAsync();
        return impacts.Select(MapToDto);
    }

    public async Task<IEnumerable<ImpactDto>> GetByDisasterEventAsync(int disasterEventId)
    {
        var impacts = await _repository.GetByDisasterEventIdAsync(disasterEventId);
        return impacts.Select(MapToDto);
    }

    public async Task<ImpactDto?> GetByIdAsync(int id)
    {
        var impact = await _repository.GetByIdAsync(id);
        return impact == null ? null : MapToDto(impact);
    }

    private static ImpactDto MapToDto(Impact impact)
    {
        return new ImpactDto
        {
            Id = impact.Id,
            DisasterEventId = impact.DisasterEventId,
            Type = impact.Type,
            Value = impact.Value,
            ObjectName = impact.ObjectName
        };
    }
}

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
}
using DisasterManagementSystem_Data.Models;
using DisasterManagementSystem_Services.Models;

public class ImpactService : IImpactService
{
    private readonly IImpactRepository _repository;
    private readonly AppDbContext _context; // Added so we can save when standalone

    public ImpactService(IImpactRepository repository, AppDbContext context)
    {
        _repository = repository;
        _context = context;
    }

    public async Task<bool> CreateImpactsAsync(IEnumerable<ImpactCreateDto> dtos, bool saveImmediately = true)
    {
        var impacts = dtos.Select(dto => new Impact
        {
            DisasterEventId = dto.DisasterEventId,
            Type = dto.Type,
            Value = dto.Value,
            ObjectName = dto.ObjectName,
            Status = dto.Status
        });

        await _repository.AddRangeAsync(impacts);
        if (saveImmediately)
            await _context.SaveChangesAsync();

        return true;
    }

    public async Task<Result<bool>> UpdateImpactAsync(int id, ImpactUpdateDto dto, bool saveImmediately = true)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing == null)
            return Result<bool>.Failure("Impact not found.");

        // Only update fields that are provided (avoid null/empty overwrite)
        if (!string.IsNullOrWhiteSpace(dto.Type)) existing.Type = dto.Type;
        if (!string.IsNullOrWhiteSpace(dto.Value)) existing.Value = dto.Value;
        if (!string.IsNullOrWhiteSpace(dto.ObjectName)) existing.ObjectName = dto.ObjectName;
        if (!string.IsNullOrWhiteSpace(dto.Status)) existing.Status = dto.Status;

        try
        {
            await _repository.UpdateAsync(existing);
            if (saveImmediately)
                await _context.SaveChangesAsync();

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"Error updating impact: {ex.Message}");
        }
    }

    public async Task<Result<bool>> DeleteImpactAsync(int id, bool saveImmediately = true)
    {
        var impact = await _repository.GetByIdAsync(id);
        if (impact == null)
            return Result<bool>.Failure("Impact not found.");

        await _repository.DeleteAsync(impact);
        if (saveImmediately)
            await _context.SaveChangesAsync();

        return Result<bool>.Success(true);
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

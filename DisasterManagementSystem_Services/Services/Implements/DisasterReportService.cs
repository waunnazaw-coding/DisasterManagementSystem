using DisasterManagementSystem_Data.Models;
using DisasterManagementSystem_Services.Models;

public class DisasterReportService : IDisasterReportService
{
    private readonly IDisasterReportRepository _repository;

    public DisasterReportService(IDisasterReportRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<DisasterReport>> GetByIdAsync(int id)
    {
        var report = await _repository.GetByIdAsync(id);
        return report != null
            ? Result<DisasterReport>.Success(report)
            : Result<DisasterReport>.NotFoundError("Report not found.");
    }

    public async Task<Result<IEnumerable<DisasterReport>>> GetAllAsync()
    {
        var all = await _repository.GetAllAsync();
        return Result<IEnumerable<DisasterReport>>.Success(all);
    }

    public async Task<Result<DisasterReport>> AddAsync(DisasterReportCreateDto dto)
    {
        var report = new DisasterReport
        {
            DisasterEventId = dto.DisasterEventId,
            UserId = dto.UserId,
            LocationId = dto.LocationId,
            AddressDetail = dto.AddressDetail,
            Type = dto.Type,
            Title = dto.Title,
            Description = dto.Description,
            Severity = dto.Severity,
            Source = dto.Source,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Status = "Pending"
        };

        await _repository.AddAsync(report);
        return Result<DisasterReport>.Success(report, "Report created.");
    }

    public async Task<Result<DisasterReport>> UpdateAsync(DisasterReportUpdateDto dto)
    {
        var existing = await _repository.GetByIdAsync(dto.Id);
        if (existing == null)
            return Result<DisasterReport>.NotFoundError("Report not found.");

        existing.AddressDetail = dto.AddressDetail;
        existing.Type = dto.Type;
        existing.Title = dto.Title;
        existing.Description = dto.Description;
        existing.Severity = dto.Severity;
        existing.Source = dto.Source;
        existing.Status = dto.Status ?? "Pending";
        existing.UpdatedAt = DateTime.UtcNow;

        await _repository.UpdateAsync(existing);
        return Result<DisasterReport>.Success(existing, "Report updated.");
    }

    public async Task<Result<bool>> DeleteAsync(int id)
    {
        var report = await _repository.GetByIdAsync(id);
        if (report == null)
            return Result<bool>.NotFoundError("Report not found.");

        await _repository.DeleteAsync(id);
        return Result<bool>.Success(true, "Report deleted.");
    }
}

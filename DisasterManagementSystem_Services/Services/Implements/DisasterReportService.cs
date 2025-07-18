using DisasterManagementSystem_Data.Models;
using DisasterManagementSystem_Data.Repositories.Interfaces;
using DisasterManagementSystem_Services.Models;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using AppLocation = DisasterManagementSystem_Data.Models.Location;

public class DisasterReportService : IDisasterReportService
{
    private readonly AppDbContext _context;
    private readonly IDisasterReportRepository _disasterReportRepository;
    private readonly IlocationRepository _locationRepository;
    private readonly IDisasterTypeRepository _disasterTypeRepository;
    private readonly IReportPhotoRepository _reportPhotoRepository;
    private readonly GeoJsonReader _geoJsonReader;

    public DisasterReportService(
        IDisasterReportRepository disasterReportRepository,
        IlocationRepository locationRepository,
        IDisasterTypeRepository disasterTypeRepository,
        IReportPhotoRepository reportPhotoRepository,
        AppDbContext context
    )
    {
        _disasterReportRepository = disasterReportRepository;
        _locationRepository = locationRepository;
        _disasterTypeRepository = disasterTypeRepository;
        _reportPhotoRepository = reportPhotoRepository;
        _context = context;
        _geoJsonReader = new GeoJsonReader();
    }

    public async Task<Result<DisasterReport>> GetByIdAsync(int id)
    {
        var report = await _disasterReportRepository.GetByIdAsync(id);
        return report != null
            ? Result<DisasterReport>.Success(report)
            : Result<DisasterReport>.NotFoundError("Report not found.");
    }

    public async Task<Result<IEnumerable<DisasterReport>>> GetAllAsync()
    {
        var all = await _disasterReportRepository.GetAllAsync();
        return Result<IEnumerable<DisasterReport>>.Success(all);
    }

    public async Task<Result<FormCreateDto>> AddFormAsync(FormCreateDto dto)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var geometry = _geoJsonReader.Read<Geometry>(dto.GeoJson);
            var location = new AppLocation
            {
                Name = dto.LocationName,
                Geography = geometry,
                Address = dto.Address,
                Country = dto.Country,
                Region = dto.Region
            };
            await _locationRepository.AddAsync(location);
            await _context.SaveChangesAsync();

            var report = new DisasterReport
            {
                DisasterEventId = dto.DisasterEventId,
                UserId = dto.UserId,
                LocationId = location.Id,
                AddressDetail = dto.AddressDetail,
                Type = dto.Type, //Situation report,...
                Title = dto.Title,
                Description = dto.Description,
                Severity = dto.Severity,
                Source = dto.Source,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Status = "Pending"
            };
            await _disasterReportRepository.AddAsync(report);
            await _context.SaveChangesAsync();

            var disasterType = new DisasterType
            {
                Name = dto.DisasterTypeName,
                Category = dto.Category,
                Description = dto.DisasterTypeDescription
            };
            await _disasterTypeRepository.AddAsync(disasterType);
            await _context.SaveChangesAsync();

            var photo = new ReportPhoto
            {
                FilePath = dto.FilePath,
                FileType = dto.FileType,
                FileSize = dto.FileSize,
                UploadedAt = dto.UploadedAt ?? DateTime.UtcNow,
                DisasterReportId = report.Id
            };
            await _reportPhotoRepository.AddAsync(photo);
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();
            return Result<FormCreateDto>.Success(dto, "Form submitted successfully.");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return Result<FormCreateDto>.Failure($"Error submitting form: {ex.Message}");
        }
    }

    public async Task<Result<DisasterReport>> UpdateAsync(DisasterReportUpdateDto dto)
    {
        var existing = await _disasterReportRepository.GetByIdAsync(dto.Id);
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

        await _disasterReportRepository.UpdateAsync(existing);
        return Result<DisasterReport>.Success(existing, "Report updated.");
    }

    public async Task<Result<bool>> DeleteAsync(int id)
    {
        var report = await _disasterReportRepository.GetByIdAsync(id);
        if (report == null)
            return Result<bool>.NotFoundError("Report not found.");

        await _disasterReportRepository.DeleteAsync(id);
        return Result<bool>.Success(true, "Report deleted.");
    }
}

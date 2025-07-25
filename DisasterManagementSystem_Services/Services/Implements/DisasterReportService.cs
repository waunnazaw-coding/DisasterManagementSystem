using DisasterManagementSystem_Data.Models;
using DisasterManagementSystem_Data.Repositories.Interfaces;
using DisasterManagementSystem_Services.Models;
using DisasterManagementSystem_Services.Models.LocationDtos;
using DisasterManagementSystem_Services.Services;
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
    private readonly IlocationService _locationService;
    private readonly IReportPhotoService _reportPhotoService;

    public DisasterReportService(
        IDisasterReportRepository disasterReportRepository,
        IlocationRepository locationRepository,
        IDisasterTypeRepository disasterTypeRepository,
        IReportPhotoRepository reportPhotoRepository,
        IlocationService locaitonService,
        IReportPhotoService reportPhotoService,
        AppDbContext context
    )
    {
        _disasterReportRepository = disasterReportRepository;
        _locationRepository = locationRepository;
        _disasterTypeRepository = disasterTypeRepository;
        _reportPhotoRepository = reportPhotoRepository;
        _context = context;
        _locationService = locaitonService;
        _geoJsonReader = new GeoJsonReader();
        _reportPhotoService = reportPhotoService;
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
            // Call LocationService to create location
            var locationResult = await _locationService.AddAsync(new LocationCreateDto
            {
                Name = dto.LocationName,
                GeoJson = dto.GeoJson,
            });

            if (!locationResult.IsSuccess)
                return Result<FormCreateDto>.Failure(locationResult.Message);

            var locationId = locationResult.Data.Id;

            // Create DisasterReport
            var report = new DisasterReport
            {
                DisasterEventId = dto.DisasterEventId,
                UserId = dto.UserId,
                LocationId = locationId,
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
            await _disasterReportRepository.AddAsync(report);
            await _context.SaveChangesAsync();

            // Create ReportPhoto
            if (dto.Files != null && dto.Files.Length > 0)
            {
                var photoResult = await _reportPhotoService.UploadReportPhotosAsync(report.Id, dto.Files);
                if (!photoResult.IsSuccess)
                    throw new Exception(photoResult.Message);
            }

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

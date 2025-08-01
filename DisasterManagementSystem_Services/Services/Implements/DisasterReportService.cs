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
    private readonly IlocationService _locationService;
    private readonly IReportPhotoService _reportPhotoService;

    public DisasterReportService(
        IDisasterReportRepository disasterReportRepository,
        IlocationService locaitonService,
        IReportPhotoService reportPhotoService,
        AppDbContext context
    )
    {
        _disasterReportRepository = disasterReportRepository;
        _context = context;
        _locationService = locaitonService;
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
                var photoResult = await _reportPhotoService.UploadReportPhotosAsync(report.Id, dto.Files, dto.NewPhotoDescriptions);
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

    public async Task<Result<FormUpdateDto>> UpdateFormAsync(FormUpdateDto dto)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            // Find existing report
            var report = await _disasterReportRepository.GetByIdAsync(dto.Id);
            if (report == null)
                return Result<FormUpdateDto>.Failure("Disaster report not found.");

            // Update Location if LocationName or GeoJson provided
            if (!string.IsNullOrEmpty(dto.LocationName) || !string.IsNullOrEmpty(dto.GeoJson))
            {
                var locationUpdateResult = await _locationService.UpdateAsync(new LocationUpdateDto
                {
                    Id = report.LocationId,
                    Name = dto.LocationName,
                    GeoJson = dto.GeoJson
                });

                if (!locationUpdateResult.IsSuccess)
                    return Result<FormUpdateDto>.Failure(locationUpdateResult.Message);
            }

            // Update DisasterReport fields if provided (null means no change)
            if (dto.DisasterEventId.HasValue) report.DisasterEventId = dto.DisasterEventId.Value;
            if (dto.UserId.HasValue) report.UserId = dto.UserId.Value;
            if (dto.AddressDetail != null) report.AddressDetail = dto.AddressDetail;
            if (dto.Type != null) report.Type = dto.Type;
            if (dto.Title != null) report.Title = dto.Title;
            if (dto.Description != null) report.Description = dto.Description;
            if (dto.Severity != null) report.Severity = dto.Severity;
            if (dto.Source != null) report.Source = dto.Source;

            report.UpdatedAt = DateTime.UtcNow;

            _context.DisasterReports.Update(report);
            await _context.SaveChangesAsync();

            // Handle photo updates if any files provided
            if (dto.Files != null && dto.Files.Length > 0)
            {
                // You can choose to replace or add photos here
                // Example: Add new photos
                var photoResult = await _reportPhotoService.UploadReportPhotosAsync(report.Id, dto.Files, dto.NewPhotoDescriptions );
                if (!photoResult.IsSuccess)
                    throw new Exception(photoResult.Message);
            }

            await transaction.CommitAsync();
            return Result<FormUpdateDto>.Success(dto, "Form updated successfully.");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return Result<FormUpdateDto>.Failure($"Error updating form: {ex.Message}");
        }
    }

    public async Task<Result<bool>> ApproveAsync(int id)
    {
        var report = await _disasterReportRepository.GetByIdAsync(id);
        if (report == null)
            return Result<bool>.NotFoundError($"Disaster report with ID {id} not found.");

        try
        {
            report.Status = "Verified";  // use Verified here
            report.UpdatedAt = DateTime.UtcNow;

            await _disasterReportRepository.UpdateAsync(report);

            return Result<bool>.Success(true, "Disaster report verified successfully.");
        }
        catch (Exception ex)
        {
            var innerMsg = ex.InnerException != null ? ex.InnerException.Message : "No inner exception";
            return Result<bool>.Failure($"Error verifying disaster report: {ex.Message}. Inner exception: {innerMsg}");
        }
    }

    public async Task<Result<bool>> DisapproveAsync(int id)
    {
        var report = await _disasterReportRepository.GetByIdAsync(id);
        if (report == null)
            return Result<bool>.NotFoundError($"Disaster report with ID {id} not found.");

        try
        {
            report.Status = "Rejected";  // Use Rejected instead of Disapproved
            report.UpdatedAt = DateTime.UtcNow;

            await _disasterReportRepository.UpdateAsync(report);

            return Result<bool>.Success(true, "Disaster report rejected successfully.");
        }
        catch (Exception ex)
        {
            var innerMsg = ex.InnerException != null ? ex.InnerException.Message : "No inner exception";
            return Result<bool>.Failure($"Error rejecting disaster report: {ex.Message}. Inner exception: {innerMsg}");
        }
    }

    public async Task<Result<bool>> DeleteAsync(int id)
    {
        var report = await _disasterReportRepository.GetByIdAsync(id);
        if (report == null)
            return Result<bool>.NotFoundError($"Disaster report with ID {id} not found.");

        try
        {
            await _disasterReportRepository.DeleteAsync(id);
            await _context.SaveChangesAsync();
            return Result<bool>.Success(true, "Disaster report deleted successfully.");
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"Error deleting disaster report: {ex.Message}");
        }
    }

}

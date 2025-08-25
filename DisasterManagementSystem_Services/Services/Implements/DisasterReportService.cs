using DisasterManagementSystem_Data.Models;
using DisasterManagementSystem_Data.Repositories.Implements;
using DisasterManagementSystem_Data.Repositories.Interfaces;
using DisasterManagementSystem_Services.Models;
using DisasterManagementSystem_Services.Models.LocationDtos;
using DisasterManagementSystem_Services.Service;
using DisasterManagementSystem_Services.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using System.Text.Json;
using AppLocation = DisasterManagementSystem_Data.Models.Location;

public class DisasterReportService : IDisasterReportService
{
  private readonly AppDbContext _context;
    private readonly IDisasterReportRepository _disasterReportRepository;
    private readonly IlocationService _locationService;
    private readonly IReportPhotoService _reportPhotoService;
    private readonly GeoJsonWriter _geoJsonWriter = new GeoJsonWriter();
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IDisasterEventService _disasterEventService;
    private readonly IDisasterTypeRepository _disasterTypeRepository;
    private readonly IDisasterTypeService _disasterTypeService;
    private readonly IReportPhotoRepository _reportPhotoRepository;

    public DisasterReportService(
        IDisasterReportRepository disasterReportRepository,
        IlocationService locationService,
        IReportPhotoService reportPhotoService,
        AppDbContext context,
        IDisasterEventService disasterEventService,
        IDisasterTypeRepository disasterTypeRepository,
        IDisasterTypeService disasterTypeService,
        IReportPhotoRepository reportPhotoRepository,
        IHttpContextAccessor httpContextAccessor
    )
    {
        _disasterReportRepository = disasterReportRepository;
        _context = context;
        _locationService = locationService;
        _reportPhotoService = reportPhotoService;
        _disasterEventService = disasterEventService;
        _disasterTypeRepository = disasterTypeRepository;
        _disasterTypeService = disasterTypeService;
        _reportPhotoRepository = reportPhotoRepository;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<Result<DisasterReportDetailsDto>> GetByIdAsync(int id)
    {
        var report = await _disasterReportRepository.GetByIdAsync(id);
        if (report == null)
            return Result<DisasterReportDetailsDto>.NotFoundError("Report not found.");

        var location = report.Location;

        LocationDto locationDto = null;
        if (location != null)
        {
            var centroid = location.Geography?.Centroid;
            double? lat = null, lon = null;

            if (centroid != null &&
                !double.IsNaN(centroid.X) && !double.IsInfinity(centroid.X) &&
                !double.IsNaN(centroid.Y) && !double.IsInfinity(centroid.Y))
            {
                lat = centroid.Y;
                lon = centroid.X;
            }

            locationDto = new LocationDto
            {
                Id = location.Id,
                Name = location.Name,
                GeoJson = location.Geography != null ? _geoJsonWriter.Write(FixPolygonOrientation(location.Geography)) : null,
                Address = location.Address,
                Country = location.Country,
                Region = location.Region,
                Latitude = lat,
                Longitude = lon
            };
        }

        var photoDtos = report.ReportPhotos.Select(photo => new ReportPhotoDto
        {
            Id = photo.Id,
            FilePath = photo.FilePath,
            Description = photo.Description,
        }).ToList();

        var dto = new DisasterReportDetailsDto
        {
            Id = report.Id,
            Title = report.Title,
            Description = report.Description,
            Type = report.Type,
            Severity = report.Severity,
            Source = report.Source,
            Status = report.Status,
            CreatedAt = report.CreatedAt,
            UpdatedAt = report.UpdatedAt,
            LocationName = locationDto.Address,
            DisasterEventId = report.DisasterEventId,
            AddressDetail = report.AddressDetail,
            LocationGeoJson = locationDto?.GeoJson,
            ReportPhotos = photoDtos
        };
        return Result<DisasterReportDetailsDto>.Success(dto);
    }

    public async Task<Result<IEnumerable<DisasterReport>>> GetAllAsync()
    {
        var all = await _disasterReportRepository.GetAllAsync();

        var reportDtos = all.Select(report => new DisasterReport
        {
            Id = report.Id,
            Title = report.Title,
            Description = report.Description,
            Type = report.Type,
            Severity = report.Severity,
            Source = report.Source,
            Status = report.Status,
            CreatedAt = report.CreatedAt,
            UpdatedAt = report.UpdatedAt,
            LocationId = report.LocationId,
            DisasterEventId = report.DisasterEventId,
            UserId = report.UserId,
            AddressDetail = report.AddressDetail,
            Location = new AppLocation
            {
                Id = report.Location?.Id ?? 0,
                Name = report.Location?.Address,
            },

        });
        return Result<IEnumerable<DisasterReport>>.Success(reportDtos);
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

            // Create ReportPhoto
            if (dto.ReportPhotos != null && dto.ReportPhotos.Length > 0)
            {
                var descriptions = dto.NewPhotoDescription ?? new List<string>();
                var uploadResult = await _reportPhotoService.UploadEventPhotosAsync
                (
                    report.Id,
                    dto.ReportPhotos,
                    descriptions
                );
                if (!uploadResult.IsSuccess)
                {
                    await transaction.RollbackAsync();
                    return Result<FormCreateDto>.Failure(uploadResult.Message);
                }
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


    public async Task<Result<ReportImpactCreateDto>> CreateAsync(ReportImpactCreateDto dto)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        var impacts = JsonSerializer.Deserialize<List<ImpactCreateDto>>(dto.ImpactsJson);

        try
        {
            // Call LocationService to create location
            var locationResult = await _locationService.AddAsync(new LocationCreateDto
            {
                Name = dto.LocationName,
                Address = dto.Address,
                Region = dto.Region,
                Country = dto.Country,
                GeoJson = dto.GeoJson,
            });

            if (!locationResult.IsSuccess)
                return Result<ReportImpactCreateDto>.Failure(locationResult.Message);

            var locationId = locationResult.Data.Id;

            // Create DisasterReport
            var report = new DisasterReport
            {
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

            // Save Impacts linked to this DisasterReport
            if (impacts != null && impacts.Any())
            {
                foreach (var impactDto in impacts)
                {
                    var impact = new Impact
                    {
                        DisasterReportId = report.Id,
                        Type = impactDto.Type,
                        Value = impactDto.Value,
                        ObjectName = impactDto.ObjectName,
                        Status = "Pending"
                    };
                    await _context.Impacts.AddAsync(impact);
                }
                await _context.SaveChangesAsync();
            }

            // Create ReportPhoto
            if (dto.ReportPhotos != null && dto.ReportPhotos.Length > 0)
            {
                var descriptions = dto.NewPhotoDescription ?? new List<string>();
                var uploadResult = await _reportPhotoService.UploadEventPhotosAsync
                (
                    report.Id,
                    dto.ReportPhotos,
                    descriptions
                );
                if (!uploadResult.IsSuccess)
                {
                    await transaction.RollbackAsync();
                    return Result<ReportImpactCreateDto>.Failure(uploadResult.Message);
                }
            }

            await transaction.CommitAsync();
            return Result<ReportImpactCreateDto>.Success(dto, "Form submitted successfully.");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return Result<ReportImpactCreateDto>.Failure($"Error submitting form: {ex.Message}");
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


            // ---------- Handle Photo Deletion ----------
            if (dto.DeletedPhotoIds.Any())
            {
                foreach (var photoId in dto.DeletedPhotoIds)
                {
                    var deleteResult = await _reportPhotoService.DeletePhotoAsync(photoId);
                    if (!deleteResult.IsSuccess)
                    {
                        await transaction.RollbackAsync();
                        return Result<FormUpdateDto>.Failure(
                            $"Failed to delete photo with ID {photoId}: {deleteResult.Message}"
                        );
                    }
                }
            }
            // ---------- Handle Updating Existing Photos' Descriptions ----------
            if (dto.ExistingPhotos != null && dto.ExistingPhotos.Count > 0)
            {
                foreach (var existingPhotoDto in dto.ExistingPhotos)
                {
                    var updateResult = await _reportPhotoService.UpdatePhotoDescriptionAsync(existingPhotoDto.Id, existingPhotoDto.Description);
                    if (!updateResult.IsSuccess)
                    {
                        await transaction.RollbackAsync();
                        return Result<FormUpdateDto>.Failure($"Failed to update photo description for photo ID {existingPhotoDto.Id}: {updateResult.Message}");
                    }
                }
            }
            // ---------- Handle Adding New Photos ----------
            if (dto.NewPhotos != null && dto.NewPhotos.Length > 0)
            {
                var descriptions = dto.NewPhotoDescription ?? new List<string>();

                var uploadResult = await _reportPhotoService.UploadEventPhotosAsync(
                    dto.Id,
                    dto.NewPhotos,
                    descriptions
                );

                if (!uploadResult.IsSuccess)
                {
                    await transaction.RollbackAsync();
                    return Result<FormUpdateDto>.Failure(
                        $"Failed to upload new photos: {uploadResult.Message}"
                    );
                }
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

    public async Task<Result<bool>> ApproveAsync(int reportId)
    {
        // 1. Get the report
        var report = await _disasterReportRepository.GetByIdAsync(reportId);
        if (report == null)
            return Result<bool>.NotFoundError($"Disaster report with ID {reportId} not found.");

        // 2. Get current user (approver)
        var currentUserIdStr = _httpContextAccessor.HttpContext?.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(currentUserIdStr))
            return Result<bool>.Failure("User is not authenticated.");
        var currentUserId = Guid.Parse(currentUserIdStr);

        try
        {
            // 3. Approve report
            report.Status = "Verified";
            report.UpdatedAt = DateTime.UtcNow;
            await _disasterReportRepository.UpdateAsync(report);

            // 4. Map report → EventFormCreateDto
            var disasterType = await _disasterTypeService.GetAllAsync(); // get all types
            var typeMatch = disasterType.Data.FirstOrDefault(dt => dt.Name == report.Type);
            if (typeMatch == null)
                return Result<bool>.Failure($"DisasterType '{report.Type}' not found.");

            var eventDto = new EventFormCreateDto
            {
                Name = report.Title ?? report.Type,
                DisasterTypeId = typeMatch.Id,
                StartDate = report.CreatedAt.HasValue ? DateOnly.FromDateTime(report.CreatedAt.Value) : DateOnly.FromDateTime(DateTime.UtcNow),
                LocationId = report.LocationId,
                Severity = report.Severity ?? "Low",
                Description = report.Description,
                Source = report.Source,
                CreatedUserId = currentUserId,
                CreatedAt = DateTime.UtcNow,
                UpdatedUserId = null,
                UpdatedAt = null
            };

            // 5. Create DisasterEvent
            var addEventResult = await _disasterEventService.ReportToEventFormAsync(eventDto);
            if (!addEventResult.IsSuccess)
                return Result<bool>.Failure($"Failed to create event: {addEventResult.Message}");

            // Populate the DTO Id with the created DisasterEvent Id
            eventDto.Id = addEventResult.Data.Id;

            // 6. Reassign report photos to the new event
            var photos = await _reportPhotoRepository.GetByReportIdAsync(report.Id);
            foreach (var photo in photos)
            {
                photo.DisasterEventId = eventDto.Id.Value;
                photo.DisasterReportId = null; // detach from report
                await _reportPhotoRepository.UpdateAsync(photo);
            }


            return Result<bool>.Success(true, "Report approved and event created successfully.");
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"Error approving report: {ex.Message}");
        }
    }

    public async Task<Result<bool>> DisapproveAsync(int reportId)
    {
        var report = await _disasterReportRepository.GetByIdAsync(reportId);
        if (report == null)
            return Result<bool>.NotFoundError($"Disaster report with ID {reportId} not found.");

        try
        {
            // Set report as rejected
            report.Status = "Rejected";
            report.UpdatedAt = DateTime.UtcNow;

            // Optional: set the user who rejected
            var currentUserId = _httpContextAccessor.HttpContext?.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(currentUserId))
                report.UserId = Guid.Parse(currentUserId);

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

    // You may need to implement this method if not already present:
    private static object FixPolygonOrientation(object geography)
    {
        // Implement polygon orientation fix logic here if needed.
        return geography;
    }

}

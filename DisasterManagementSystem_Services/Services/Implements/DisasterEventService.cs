using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using DisasterManagementSystem_Data.Models;
using DisasterManagementSystem_Data.Repositories;
using DisasterManagementSystem_Services.Models;
using DisasterManagementSystem_Services.Models.LocationDtos;
using DisasterManagementSystem_Services.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.IO;

namespace DisasterManagementSystem_Data.Service
{
    public class DisasterEventService : IDisasterEventService
    {
        private readonly AppDbContext _context;
        private readonly IDisasterEventRepository _disasterEventRepository;
        private readonly IlocationService _locationService;
        private readonly IReportPhotoService _reportPhotoService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly GeoJsonWriter _geoJsonWriter = new GeoJsonWriter();

        public DisasterEventService(
            AppDbContext context,
            IDisasterEventRepository disasterEventRepository,
            IlocationService locationService,
            IReportPhotoService reportPhotoService,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _disasterEventRepository = disasterEventRepository;
            _locationService = locationService;
            _reportPhotoService = reportPhotoService;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<Result<DisasterEventListDto>> GetByIdAsync(int id)
        {
            var disasterEvent = await _disasterEventRepository.GetByIdAsync(id);
            if (disasterEvent == null)
                return Result<DisasterEventListDto>.Failure("Disaster event not found.");

            var affectedPeople = disasterEvent.Impacts?
                .Where(i => i.Type == "Casualties" || i.Type == "Displacement")
                .Sum(i => int.TryParse(i.Value, out var val) ? val : 0) ?? 0;

            var dto = new DisasterEventListDto
            {
                Id = disasterEvent.Id,
                Name = disasterEvent.Name,
                DisasterTypeName = disasterEvent.DisasterType?.Name ?? "Unknown",
                StartDate = disasterEvent.StartDate,
                LocationName = disasterEvent.Location?.Name ?? "Unknown",
                Region = disasterEvent.Location?.Region,
                Country = disasterEvent.Location?.Country,
                Severity = disasterEvent.Severity,
                Status = disasterEvent.Status,
                Description = disasterEvent.Description,
                CreatedUserId = disasterEvent.CreatedUserId,
                CreatedAt = disasterEvent.CreatedAt,
                UpdatedUserId = disasterEvent.UpdatedUserId,
                UpdatedAt = disasterEvent.UpdatedAt,
                AffectedPeople = affectedPeople
            };

            return Result<DisasterEventListDto>.Success(dto);
        }

        public async Task<Result<DisasterEventListDto>> GetByIdWithLocationAsync(int eventId)
        {
            var disasterEvent = await _disasterEventRepository.GetByIdAsync(eventId);
            if (disasterEvent == null)
                return Result<DisasterEventListDto>.NotFoundError("Event not found.");

            var affectedPeople = disasterEvent.Impacts?
              .Where(i => i.Type == "Casualties" || i.Type == "Displacement")
              .Sum(i => int.TryParse(i.Value, out var val) ? val : 0) ?? 0;

            var location = disasterEvent.Location;

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

            var dto = new DisasterEventListDto
            {
                Id = disasterEvent.Id,
                Name = disasterEvent.Name,
                DisasterTypeName = disasterEvent.DisasterType?.Name,
                StartDate = disasterEvent.StartDate,
                LocationName = location?.Name,
                Region = location?.Region,
                Country = location?.Country,
                Severity = disasterEvent.Severity,
                Status = disasterEvent.Status,
                Description = disasterEvent.Description,
                AffectedPeople = affectedPeople,
                LocationGeoJson = locationDto?.GeoJson
            };

            return Result<DisasterEventListDto>.Success(dto);
        }

        public async Task<Result<EventFormUpdateDto>> GetByIdForUpdateAsync(int eventId)
        {
            var disasterEvent = await _disasterEventRepository.GetByIdAsync(eventId);
            if (disasterEvent == null)
                return Result<EventFormUpdateDto>.NotFoundError("Event not found");

            var location = disasterEvent.Location;

            var photoResult = await _reportPhotoService.GetPhotosByEventIdAsync(eventId);

            List<ExistingPhotoUpdateDto> existingPhotosMapped = new();

            if (photoResult.IsSuccess && photoResult.Data != null)
            {
                var request = _httpContextAccessor.HttpContext?.Request;
                var baseUrl = request != null ? $"{request.Scheme}://{request.Host}" : "";

                existingPhotosMapped = photoResult.Data.Select(p =>
                {
                    var filePath = p.FilePath;
                    if (!string.IsNullOrEmpty(filePath) && !filePath.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                    {
                        filePath = $"{baseUrl}{filePath}";
                    }

                    return new ExistingPhotoUpdateDto
                    {
                        Id = p.Id,
                        Description = p.Description,
                        FilePath = filePath,
                    };
                }).ToList();

            }

            return Result<EventFormUpdateDto>.Success(new EventFormUpdateDto
            {
                Id = disasterEvent.Id,
                Name = disasterEvent.Name,
                DisasterTypeId = disasterEvent.DisasterTypeId,
                StartDate = disasterEvent.StartDate,
                Severity = disasterEvent.Severity,
                Description = disasterEvent.Description,
                LocationName = location?.Name,
                GeoJson = location?.Geography != null ? _geoJsonWriter.Write(FixPolygonOrientation(location.Geography)) : null,
                ExistingPhotos = existingPhotosMapped,
            });
        }

        public async Task<Result<IEnumerable<DisasterEventListDto>>> GetAllAsync()
        {
            var events = await _disasterEventRepository.GetAllAsync();

            var eventDtos = events.Select(e => new DisasterEventListDto
            {
                Id = e.Id,
                Name = e.Name,
                DisasterTypeName = e.DisasterType?.Name ?? "Unknown",
                StartDate = e.StartDate,
                LocationName = e.Location?.Name ?? "Unknown",
                Region = e.Location?.Region,
                Country = e.Location?.Country,
                Severity = e.Severity,
                Status = e.Status,
                Description = e.Description,
                CreatedUserId = e.CreatedUserId,
                CreatedAt = e.CreatedAt,
                UpdatedUserId = e.UpdatedUserId,
                UpdatedAt = e.UpdatedAt
            });

            return Result<IEnumerable<DisasterEventListDto>>.Success(eventDtos);
        }

        public async Task<List<DisasterEventListDto>> GetAllWithAffectedPeopleAsync()
        {
            var events = await _context.DisasterEvents
                .Include(e => e.DisasterType)
                .Include(e => e.Location)
                .ToListAsync();

            var impacts = await _context.Impacts
                .Where(i => i.Type == "Casualties" || i.Type == "Displacement")
                .ToListAsync();

            return events.Select(de =>
            {
                var affectedPeople = impacts
                    .Where(i => i.DisasterEventId == de.Id)
                    .Sum(i => int.TryParse(i.Value, out var val) ? val : 0);

                return new DisasterEventListDto
                {
                    Id = de.Id,
                    Name = de.Name,
                    DisasterTypeName = de.DisasterType?.Name ?? "Unknown",
                    StartDate = de.StartDate,
                    LocationName = de.Location?.Name ?? "Unknown",
                    Region = de.Location?.Region,
                    Country = de.Location?.Country,
                    Severity = de.Severity,
                    Status = de.Status,
                    Description = de.Description,
                    CreatedUserId = de.CreatedUserId,
                    CreatedAt = de.CreatedAt,
                    UpdatedUserId = de.UpdatedUserId,
                    UpdatedAt = de.UpdatedAt,
                    AffectedPeople = affectedPeople
                };
            }).ToList();
        }

        public async Task<IEnumerable<DisasterEventListDto>> SearchByNameAsync(string name)
        {
            var events = await _disasterEventRepository.SearchByNameAsync(name);

            return events.Select(e => new DisasterEventListDto
            {
                Id = e.Id,
                Name = e.Name,
                DisasterTypeName = e.DisasterType?.Name ?? "Unknown",
                StartDate = e.StartDate,
                LocationName = e.Location?.Name ?? "Unknown",
                Severity = e.Severity,
                Status = e.Status,
                Description = e.Description,
                CreatedUserId = e.CreatedUserId,
                CreatedAt = e.CreatedAt,
                UpdatedUserId = e.UpdatedUserId,
                UpdatedAt = e.UpdatedAt
            });
        }

        public async Task<Result<EventFormCreateDto>> AddEventFormAsync(EventFormCreateDto dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 1. Create Location
                var locationResult = await _locationService.AddAsync(new LocationCreateDto
                {
                    Name = dto.LocationName,
                    GeoJson = dto.GeoJson,
                });

                if (!locationResult.IsSuccess)
                    return Result<EventFormCreateDto>.Failure(locationResult.Message);

                dto.LocationId = locationResult.Data.Id;

                // 2. Get current user
                var currentUserId = _httpContextAccessor.HttpContext?.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(currentUserId))
                    return Result<EventFormCreateDto>.Failure("User is not authenticated.");

                // 3. Create DisasterEvent
                var disasterEvent = new DisasterEvent
                {
                    Name = dto.Name,
                    DisasterTypeId = dto.DisasterTypeId,
                    StartDate = dto.StartDate,
                    LocationId = dto.LocationId,
                    Severity = dto.Severity,
                    Status = "Active",
                    Description = dto.Description,
                    CreatedUserId = Guid.Parse(currentUserId),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedUserId = null,
                    UpdatedAt = null
                };

                await _disasterEventRepository.AddAsync(disasterEvent);

                // 4. Upload and link photos
                if (dto.ReportPhotos != null && dto.ReportPhotos.Any())
                {
                    var uploadResult = await _reportPhotoService.UploadEventPhotosAsync(disasterEvent.Id, dto.ReportPhotos, dto.NewPhotoDescription);
                    if (!uploadResult.IsSuccess)
                    {
                        await transaction.RollbackAsync();
                        return Result<EventFormCreateDto>.Failure(uploadResult.Message);
                    }

                }

                await transaction.CommitAsync();
                return Result<EventFormCreateDto>.Success(dto);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Result<EventFormCreateDto>.Failure($"Error creating disaster event: {ex.Message}");
            }
        }

        public async Task<Result<EventFormUpdateDto>> UpdateEventFormAsync(EventFormUpdateDto dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var disasterEvent = await _disasterEventRepository.GetByIdAsync(dto.Id);
                if (disasterEvent == null)
                    return Result<EventFormUpdateDto>.Failure("Disaster event not found.");

                // Update location via service
                var locationUpdateResult = await _locationService.UpdateAsync(new LocationUpdateDto
                {
                    Id = disasterEvent.LocationId,
                    Name = dto.LocationName,
                    GeoJson = dto.GeoJson
                });


                if (!locationUpdateResult.IsSuccess)
                    return Result<EventFormUpdateDto>.Failure(locationUpdateResult.Message);

                // Get user for UpdatedUserId
                var currentUserId = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(currentUserId))
                    return Result<EventFormUpdateDto>.Failure("User is not authenticated.");

                // Update disaster event fields
                disasterEvent.Name = dto.Name;
                disasterEvent.DisasterTypeId = dto.DisasterTypeId;
                disasterEvent.StartDate = dto.StartDate ?? disasterEvent.StartDate;
                disasterEvent.Severity = dto.Severity;
                disasterEvent.Description = dto.Description;
                disasterEvent.UpdatedUserId = Guid.Parse(currentUserId);
                disasterEvent.UpdatedAt = DateTime.UtcNow;

                await _disasterEventRepository.UpdateAsync(disasterEvent);

                // ---------- Handle Photo Deletion ----------
                if (dto.DeletedPhotoIds.Any())
                {
                    foreach (var photoId in dto.DeletedPhotoIds)
                    {
                        var deleteResult = await _reportPhotoService.DeletePhotoAsync(photoId);
                        if (!deleteResult.IsSuccess)
                        {
                            await transaction.RollbackAsync();
                            return Result<EventFormUpdateDto>.Failure(
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
                            return Result<EventFormUpdateDto>.Failure($"Failed to update photo description for photo ID {existingPhotoDto.Id}: {updateResult.Message}");
                        }
                    }
                }

                // ---------- Handle Adding New Photos ----------
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
                        return Result<EventFormUpdateDto>.Failure(
                            $"Failed to upload new photos: {uploadResult.Message}"
                        );
                    }
                }


                await transaction.CommitAsync();
                return Result<EventFormUpdateDto>.Success(dto);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Result<EventFormUpdateDto>.Failure($"Error updating disaster event: {ex.Message}");
            }
        }

        public async Task<Result<bool>> DeleteAsync(int id)
        {
            var disasterEvent = await _disasterEventRepository.GetByIdAsync(id);
            if (disasterEvent == null)
                return Result<bool>.NotFoundError($"Disaster event with ID {id} not found.");

            await _disasterEventRepository.DeleteAsync(id);
            return Result<bool>.Success(true, "Event deleted successfully.");
        }

        // You may need to implement this method if not already present:
        private static object FixPolygonOrientation(object geography)
        {
            // Implement polygon orientation fix logic here if needed.
            return geography;
        }

        private static double? SanitizeNullableDouble(double? value)
        {
            if (value == null) return null;
            if (double.IsNaN(value.Value) || double.IsInfinity(value.Value)) return null;
            return value;
        }
    }
}

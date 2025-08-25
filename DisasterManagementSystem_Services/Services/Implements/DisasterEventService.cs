using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using DisasterManagementSystem_Data.Models;
using DisasterManagementSystem_Data.Repositories;
using DisasterManagementSystem_Data.Repositories.Interfaces;
using DisasterManagementSystem_Services.Models;
using DisasterManagementSystem_Services.Models.LocationDtos;
using DisasterManagementSystem_Services.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.IO;

namespace DisasterManagementSystem_Services.Service
{
    public class DisasterEventService : IDisasterEventService
    {
        private readonly AppDbContext _context;
        private readonly IDisasterEventRepository _disasterEventRepository;
        private readonly IImpactRepository _impactRepository;
        private readonly IlocationRepository _locationRepository;
        private readonly IReportPhotoRepository _reportPhotoRepository;
        private readonly IlocationService _locationService;
        private readonly IImpactService _impactService;
        private readonly IReportPhotoService _reportPhotoService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly GeoJsonWriter _geoJsonWriter = new GeoJsonWriter();

        public DisasterEventService(
            AppDbContext context,
            IDisasterEventRepository disasterEventRepository,
            IlocationService locationService,
            IReportPhotoService reportPhotoService,
            IImpactService impactService,
            IHttpContextAccessor httpContextAccessor,
            IImpactRepository impactRepository,
            IlocationRepository locationRepository,
            IReportPhotoRepository reportPhotoRepository
        )
        {
            _context = context;
            _disasterEventRepository = disasterEventRepository;
            _locationService = locationService;
            _reportPhotoService = reportPhotoService;
            _httpContextAccessor = httpContextAccessor;
            _impactRepository = impactRepository;
            _locationRepository = locationRepository;
            _reportPhotoRepository = reportPhotoRepository;
            _impactService = impactService;
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
                Source = disasterEvent.Source,
                Description = disasterEvent.Description,
                CreatedUserId = disasterEvent.CreatedUserId,
                CreatedAt = disasterEvent.CreatedAt,
                UpdatedUserId = disasterEvent.UpdatedUserId,
                UpdatedAt = disasterEvent.UpdatedAt,
                AffectedPeople = affectedPeople
            };

            return Result<DisasterEventListDto>.Success(dto);
        }

        public async Task<Result<DisasterEventDetailsDto>> GetByIdWithLocationAsync(int eventId)
        {
            var disasterEvent = await _disasterEventRepository.GetByIdAsync(eventId);
            if (disasterEvent == null)
                return Result<DisasterEventDetailsDto>.NotFoundError("Event not found.");

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

            var impacts = disasterEvent.Impacts ?? new List<Impact>();

            var impactSummaries = impacts
                .GroupBy(i => new { i.Type, i.ObjectName })
                .Select(g =>
                {
                    decimal sum = 0;
                    var descriptions = new List<string>();

                    foreach (var impact in g)
                    {
                        if (decimal.TryParse(impact.Value, out var val))
                        {
                            sum += val;
                        }
                        else if (!string.IsNullOrWhiteSpace(impact.Value))
                        {
                            descriptions.Add(impact.Value);
                        }
                    }

                    return new ImpactSummaryDto
                    {
                        Type = g.Key.Type,
                        ObjectName = g.Key.ObjectName,
                        TotalValue = sum,
                        Descriptions = descriptions
                    };
                }).ToList();

            var photoDtos = disasterEvent.ReportPhotos.Select(photo => new ReportPhotoDto
            {
                Id = photo.Id,
                FilePath = photo.FilePath,
                Description = photo.Description
            }).ToList();

            var dto = new DisasterEventDetailsDto
            {
                Id = disasterEvent.Id,
                Name = disasterEvent.Name,
                DisasterTypeName = disasterEvent.DisasterType?.Name,
                StartDate = disasterEvent.StartDate,
                LocationName = location?.Address,
                Region = location?.Region,
                Country = location?.Country,
                Severity = disasterEvent.Severity,
                Status = disasterEvent.Status,
                Source = disasterEvent.Source,
                Description = disasterEvent.Description,
                LocationGeoJson = locationDto?.GeoJson,
                ImpactSummaries = impactSummaries,
                ReportPhotos = photoDtos,
                CreatedUserName = disasterEvent.CreatedUser?.Name ?? "Unknown",
                CreatedAt = disasterEvent.CreatedAt,
                UpdatedUserName = disasterEvent.UpdatedUser?.Name ?? "N/A",
                UpdatedAt = disasterEvent.UpdatedAt
            };

            return Result<DisasterEventDetailsDto>.Success(dto);
        }

        public async Task<Result<EventFormUpdateDto>> GetByIdForUpdateAsync(int eventId)
        {
            var disasterEvent = await _disasterEventRepository.GetByIdAsync(eventId);
            if (disasterEvent == null)
                return Result<EventFormUpdateDto>.NotFoundError("Event not found");

            var location = disasterEvent.Location;

            var photoResult = await _reportPhotoService.GetPhotosByEventIdAsync(eventId);

            var impactResult = await _impactService.GetByDisasterEventAsync(eventId);

            List<ExistingPhotoUpdateDto> existingPhotosMapped = new();
            List<ImpactUpdateDto> existingImpactsMapped = new();

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

            if (impactResult != null)
            {
                existingImpactsMapped = impactResult.Select(i => new ImpactUpdateDto
                {
                    Id = i.Id,
                    Type = i.Type,
                    Value = i.Value,
                    ObjectName = i.ObjectName,
                }).ToList();
            }

            return Result<EventFormUpdateDto>.Success(new EventFormUpdateDto
            {
                Id = disasterEvent.Id,
                Name = disasterEvent.Name,
                DisasterTypeId = disasterEvent.DisasterTypeId,
                StartDate = disasterEvent.StartDate,
                Source = disasterEvent.Source,
                Severity = disasterEvent.Severity,
                Description = disasterEvent.Description,
                LocationName = location?.Name,
                GeoJson = location?.Geography != null ? _geoJsonWriter.Write(FixPolygonOrientation(location.Geography)) : null,
                ExistingPhotos = existingPhotosMapped,
                ExistingImpacts = existingImpactsMapped,
                DeletedImpactIds = new List<int>(),
                DeletedPhotoIds = new List<int>(),
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
                Source = e.Source,
                CreatedUserId = e.CreatedUserId,
                CreatedAt = e.CreatedAt,
                UpdatedUserId = e.UpdatedUserId,
                UpdatedAt = e.UpdatedAt
            });

            return Result<IEnumerable<DisasterEventListDto>>.Success(eventDtos);
        }

        public async Task<Result<IEnumerable<DisasterEventListDto>>> GetAllActiveAsync()
        {
            var events = await _disasterEventRepository.GetAllActive();

            var eventDtos = events.Select(e => new DisasterEventListDto
            {
                Id = e.Id,
                Name = e.Name,
                DisasterTypeName = e.DisasterType?.Name ?? "Unknown",
                StartDate = e.StartDate,
                LocationName = e.Location?.Name ?? "Unknown",
                Region = e.Location?.Region,
                Country = e.Location?.Country,
                Address = e.Location?.Address,
                Severity = e.Severity,
                Status = e.Status,
                Description = e.Description,
                Source = e.Source,
                CreatedUserId = e.CreatedUserId,
                CreatedAt = e.CreatedAt,
                UpdatedUserId = e.UpdatedUserId,
                UpdatedAt = e.UpdatedAt,
                LocationGeoJson = e.Location?.Geography != null
                    ? _geoJsonWriter.Write(FixPolygonOrientation(e.Location.Geography))
                    : null
            }).ToList();
            return Result<IEnumerable<DisasterEventListDto>>.Success(eventDtos);
        }
        public async Task<Result<IEnumerable<DisasterEventListDto>>> GetAllForMapViewAsync()
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
                Address = e.Location?.Address,
                Severity = e.Severity,
                Status = e.Status,
                Description = e.Description,
                Source = e.Source,
                CreatedUserId = e.CreatedUserId,
                CreatedUserName = e.CreatedUser?.Name ?? "Unknown",
                CreatedAt = e.CreatedAt,
                UpdatedUserId = e.UpdatedUserId,
                UpdatedAt = e.UpdatedAt,
                LocationGeoJson = e.Location?.Geography != null
                    ? _geoJsonWriter.Write(FixPolygonOrientation(e.Location.Geography))
                    : null
            }).ToList();


            return Result<IEnumerable<DisasterEventListDto>>.Success(eventDtos);
        }

        public async Task<List<DisasterEventListDto>> GetAllWithAffectedPeopleAsync()
        {
            var events = await _context.DisasterEvents
                .Include(e => e.DisasterType)
                .Include(e => e.Location)
                .Include(e => e.CreatedUser)
                .Include(e => e.UpdatedUser)
                .ToListAsync();

            var impacts = await _context.Impacts
                .Where(i => i.Type == "Casualties"
                         || i.Type == "Displacement"
                         || i.Type == "Infrastructure Damage"
                         || i.Type == "Economic Loss")
                .ToListAsync();

            var photos = await _context.ReportPhotos
                .Where(p => p.DisasterEventId != null)
                .ToListAsync();

            return events.Select(de =>
            {
                var affectedPeople = impacts
                    .Where(i => i.DisasterEventId == de.Id && (i.Type == "Casualties" || i.Type == "Displacement"))
                    .Sum(i => int.TryParse(i.Value, out var val) ? val : 0);

                var affectedFamilies = impacts
                    .Where(i => i.DisasterEventId == de.Id && i.Type == "Displacement")
                    .Sum(i => int.TryParse(i.Value, out var val) ? val : 0);

                var affectedInfrastructures = impacts
                    .Where(i => i.DisasterEventId == de.Id && i.Type == "Infrastructure Damage")
                    .Sum(i => int.TryParse(i.Value, out var val) ? val : 0);

                var currencyChanges = impacts
                    .Where(i => i.DisasterEventId == de.Id && i.Type == "Economic Loss")
                    .GroupBy(i => i.ObjectName)
                    .Select(g =>
                    {
                        // Sum all values with the same ObjectName
                        var sumValue = g.Sum(i => int.TryParse(i.Value, out var val) ? val : 0);
                        var objectName = g.Key ?? "";
                        return $"{sumValue} {objectName}";
                    })
                    .ToList();


                var firstPhotoUrl = photos
                    .Where(p => p.DisasterEventId == de.Id)
                    .Select(p => p.FilePath)
                    .FirstOrDefault();


                return new DisasterEventListDto
                {
                    Id = de.Id,
                    Name = de.Name,
                    DisasterTypeName = de.DisasterType?.Name ?? "Unknown",
                    StartDate = de.StartDate,
                    LocationName = de.Location?.Name ?? "Unknown",
                    Region = de.Location?.Region,
                    Country = de.Location?.Country,
                    Address = de.Location?.Address,
                    Severity = de.Severity,
                    Status = de.Status,
                    Description = de.Description,
                    CreatedUserName = de.CreatedUser?.Name ?? "Unknown",
                    CreatedAt = de.CreatedAt,
                    UpdatedUserName = de.UpdatedUser?.Name ?? "N/A",
                    UpdatedAt = de.UpdatedAt,
                    AffectedPeople = affectedPeople,
                    AffectedFamilies = affectedFamilies,
                    AffectedInfrastructures = affectedInfrastructures,
                    CurrencyChanges = currencyChanges,
                    FirstImageUrl = firstPhotoUrl,
                    Source = de.Source
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
                Source = e.Source,
                Address = e.Location?.Address,
                Description = e.Description,
                CreatedUserId = e.CreatedUserId,
                CreatedAt = e.CreatedAt,
                UpdatedUserId = e.UpdatedUserId,
                UpdatedAt = e.UpdatedAt
            });
        }

        public async Task<Result<int>> GetActiveCountAsync()
        {
            var count = await _disasterEventRepository.CountVerifiedAsync();
            return Result<int>.Success(count);
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
                    UpdatedAt = null,
                    Source = dto.Source
                };

                await _disasterEventRepository.AddAsync(disasterEvent);

                // 4. Upload and link photos
                if (dto.ReportPhotos != null && dto.ReportPhotos.Length > 0)
                {
                    var descriptions = dto.NewPhotoDescription ?? new List<string>();
                    var uploadResult = await _reportPhotoService.UploadEventPhotosAsync
                    (
                        disasterEvent.Id,
                        dto.ReportPhotos,
                        descriptions
                    );
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
                disasterEvent.Source = dto.Source;
                disasterEvent.Status = dto.Status ?? "Active";
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

                // ---------- Handle Impact Deletion ----------
                if (dto.DeletedImpactIds.Any())
                {
                    foreach (var impactId in dto.DeletedImpactIds)
                    {
                        var deleteResult = await _impactService.DeleteImpactAsync(impactId);
                        if (!deleteResult.IsSuccess)
                        {
                            await transaction.RollbackAsync();
                            return Result<EventFormUpdateDto>.Failure(
                                $"Failed to delete impact with ID {impactId}: {deleteResult.Message}"
                            );
                        }
                    }
                }

                // ---------- Handle Updating Existing Impacts ----------
                if (dto.ExistingImpacts != null && dto.ExistingImpacts.Count > 0)
                {
                    foreach (var impactDto in dto.ExistingImpacts)
                    {
                        var updateResult = await _impactService.UpdateImpactAsync(impactDto.Id, impactDto);
                        if (!updateResult.IsSuccess)
                        {
                            await transaction.RollbackAsync();
                            return Result<EventFormUpdateDto>.Failure(
                                $"Failed to update impact with ID {impactDto.Id}: {updateResult.Message}"
                            );
                        }
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

        public async Task<Result<EventFormCreateDto>> ReportToEventFormAsync(EventFormCreateDto dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 1. Validate LocationId
                if (dto.LocationId <= 0)
                    return Result<EventFormCreateDto>.Failure("LocationId is required to create an event.");

                // 2. Create DisasterEvent using the provided data
                var disasterEvent = new DisasterEvent
                {
                    Name = dto.Name,
                    DisasterTypeId = dto.DisasterTypeId,
                    StartDate = dto.StartDate,
                    LocationId = dto.LocationId, // reuse existing location
                    Severity = dto.Severity,
                    Description = dto.Description,
                    Source = dto.Source,
                    CreatedUserId = dto.CreatedUserId,
                    CreatedAt = dto.CreatedAt,
                    UpdatedUserId = dto.UpdatedUserId,
                    UpdatedAt = dto.UpdatedAt
                };

                _context.DisasterEvents.Add(disasterEvent);
                await _context.SaveChangesAsync();

                // 3. Populate DTO with the new DisasterEvent Id
                dto.Id = disasterEvent.Id;

                // 4. Commit transaction
                await transaction.CommitAsync();

                return Result<EventFormCreateDto>.Success(dto, "Report successfully converted to event.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Result<EventFormCreateDto>.Failure($"Error converting report to event: {ex.Message}");
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
            if (geography == null) return null;
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

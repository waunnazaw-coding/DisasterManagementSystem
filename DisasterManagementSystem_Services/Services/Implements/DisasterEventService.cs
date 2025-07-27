using System.Collections.Generic;
using System.Threading.Tasks;
using DisasterManagementSystem_Data.Models;
using DisasterManagementSystem_Data.Repositories;
using DisasterManagementSystem_Services.Models;
using DisasterManagementSystem_Services.Models.LocationDtos;
using DisasterManagementSystem_Services.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace DisasterManagementSystem_Data.Service
{
    public class DisasterEventService : IDisasterEventService
    {
        private readonly AppDbContext _context;
        private readonly IDisasterEventRepository _disasterEventRepository;
        private readonly IlocationService _locationService;
        private readonly IReportPhotoService _reportPhotoService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public DisasterEventService(
            AppDbContext context,
            IDisasterEventRepository disasterEventRepository,
            IlocationService locationService,
            IReportPhotoService reportPhotoService,
            IHttpContextAccessor httpContextAccessor
        )
        {
            _context = context;
            _disasterEventRepository = disasterEventRepository;
            _locationService = locationService;
            _reportPhotoService = reportPhotoService;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<Result<DisasterEvent>> GetByIdAsync(int id)
        {
            var disasterEvent = await _disasterEventRepository.GetByIdAsync(id);
            return disasterEvent != null
                ? Result<DisasterEvent>.Success(disasterEvent)
                : Result<DisasterEvent>.Failure("Disaster event not found.");
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

        public async Task<IEnumerable<DisasterEventListDto>> SearchByNameAsync(string name)
        {
            var events = await _disasterEventRepository.SearchByNameAsync(name);

            return events.Select(e => new DisasterEventListDto
            {
                Id = e.Id,
                Name = e.Name,
                DisasterTypeName = e.DisasterType.Name,
                StartDate = e.StartDate,
                LocationName = e.Location.Name,
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
                // Call LocationService to create location
                var locationResult = await _locationService.AddAsync(new LocationCreateDto
                {
                    Name = dto.LocationName,
                    GeoJson = dto.GeoJson,
                });

                if (!locationResult.IsSuccess)
                    return Result<EventFormCreateDto>.Failure(locationResult.Message);

                dto.LocationId = locationResult.Data.Id;

                // Get the current user ID (from HttpContext)
                string? currentUserId = _httpContextAccessor.HttpContext?.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                // Add disaster event
                var disasterEvent = new DisasterEvent
                {
                    Name = dto.Name,
                    DisasterTypeId = dto.DisasterTypeId,
                    StartDate = dto.StartDate,
                    LocationId = dto.LocationId,
                    Severity = dto.Severity,
                    Status = "Active",
                    Description = dto.Description,
                    CreatedUserId = Guid.Parse(currentUserId!),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedUserId = null,  // ensure null
                    UpdatedAt = null       // ensure null
                };

                await _disasterEventRepository.AddAsync(disasterEvent);
                await transaction.CommitAsync();

                return Result<EventFormCreateDto>.Success(dto);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Result<EventFormCreateDto>.Failure($"Error creating disaster event: {ex.Message}");
            }
        }


        public async Task<Result<DisasterEvent>> UpdateAsync(DisasterEvent disasterEvent)
        {
            _context.DisasterEvents.Update(disasterEvent);
            await _context.SaveChangesAsync();
            return Result<DisasterEvent>.Success(disasterEvent);
        }

        public async Task<Result<bool>> DeleteAsync(int id)
        {
            var disasterEvent = await _disasterEventRepository.GetByIdAsync(id);
            if (disasterEvent != null)
            {
                await _disasterEventRepository.DeleteAsync(id);
                return Result<bool>.Success(true, "Event deleted successfully.");
            }
            else
            {
                return Result<bool>.NotFoundError($"Disaster event with ID {id} not found.");
            }
        }
    }
}
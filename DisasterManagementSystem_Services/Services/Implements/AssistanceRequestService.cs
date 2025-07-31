using DisasterManagementSystem_Data.Models;
using DisasterManagementSystem_Data.Repositories;
using DisasterManagementSystem_Data.Repositories.Interfaces;
using DisasterManagementSystem_Services.Models;
using DisasterManagementSystem_Services.Models.AssistanceRequestDtos;
using DisasterManagementSystem_Services.Models.AssistanceRequestDtos.DisasterManagementSystem_Service.Models.Dtos;
using DisasterManagementSystem_Services.Services.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisasterManagementSystem_Services.Services.Implements
{
    public class AssistanceRequestService : IAssistanceRequestService
    {
        private readonly IAssistanceRequestRepository _requestRepository;
        private readonly IUserRepository _userRepository;
        private readonly IDisasterReportRepository _disasterReportRepository;
        private readonly IDisasterEventRepository _disasterEventRepository;
        private readonly IlocationRepository _locationRepository;
        private readonly ILogger<AssistanceRequestService> _logger;
        private readonly INotificationService _notificationService;

        public AssistanceRequestService(
           IAssistanceRequestRepository requestRepository,
           IUserRepository userRepository,
           IDisasterReportRepository disasterReportRepository,
           IDisasterEventRepository disasterEventRepository,
           IlocationRepository locationRepository,
           INotificationService notificationService,
           ILogger<AssistanceRequestService> logger)
        {
            _requestRepository = requestRepository;
            _userRepository = userRepository;
            _disasterReportRepository = disasterReportRepository;
            _disasterEventRepository = disasterEventRepository;
            _locationRepository = locationRepository;
                _notificationService = notificationService;
            _logger = logger;
        }
     
        public async Task<Result<AssistanceRequestDto>> CreateRequestAsync(
    CreateAssistanceRequestDto createRequestDto,
    Guid userId)
        {
            try
            {
                // Validate required fields
                var validationResult = ValidateCreateRequest(createRequestDto);
                if (validationResult != null) return validationResult;

                // Verify user exists
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("User {UserId} not found", userId);
                    return Result<AssistanceRequestDto>.NotFoundError("User not found");
                }

                // Verify referenced entities exist
                var referenceCheck = await VerifyReferencesExist(createRequestDto);
                if (referenceCheck != null) return referenceCheck;

                // Create and save request
                var request = new AssistanceRequest
                {
                    DisasterEventId = createRequestDto.DisasterEventId,
                    DisasterReportId = createRequestDto.DisasterReportId,
                    UserId = userId,
                    LocationId = createRequestDto.LocationId,
                    SupportType = createRequestDto.SupportType,
                    Quantity = createRequestDto.Quantity,
                    Unit = createRequestDto.Unit,
                    Description = createRequestDto.Description,
                    Priority = createRequestDto.Priority,
                    Status = "Pending",
                    ContactName = createRequestDto.ContactName,
                    Email = createRequestDto.Email,
                    ContactPhone = createRequestDto.ContactPhone,
                    DetailedAddress = createRequestDto.DetailedAddress,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var createdRequest = await _requestRepository.AddAsync(request);
                // Notify admins about the new request
                await _notificationService.NotifyAdminsForNewRequest(
                    userId,
                    createdRequest.Id,
                    createRequestDto.SupportType);

                // Explicitly load related entities
                await _requestRepository.LoadRelatedEntitiesAsync(createdRequest);

                var responseDto = MapToDto(createdRequest, user);
                return Result<AssistanceRequestDto>.Success(responseDto, "Request created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating request");
                return Result<AssistanceRequestDto>.Failure("Error creating request");
            }
        }

        private async Task<Result<AssistanceRequestDto>?> VerifyReferencesExist(CreateAssistanceRequestDto dto)
        {
            if (dto.DisasterEventId.HasValue)
            {
                var disasterEvent = await _disasterEventRepository.GetByIdAsync(dto.DisasterEventId.Value);
                if (disasterEvent == null)
                {
                    return Result<AssistanceRequestDto>.ValidationError("Disaster event not found");
                }
            }

            if (dto.DisasterReportId.HasValue &&
                !await _disasterReportRepository.ExistsAsync(dto.DisasterReportId.Value))
            {
                return Result<AssistanceRequestDto>.ValidationError("Disaster report not found");
            }

            if (dto.LocationId.HasValue &&
                !await _locationRepository.ExistsAsync(dto.LocationId.Value))
            {
                return Result<AssistanceRequestDto>.ValidationError("Location not found");
            }

            return null;
        }
        private Result<AssistanceRequestDto>? ValidateCreateRequest(CreateAssistanceRequestDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.SupportType))
                return Result<AssistanceRequestDto>.ValidationError("Support type is required");

            if (string.IsNullOrWhiteSpace(dto.ContactName))
                return Result<AssistanceRequestDto>.ValidationError("Contact name is required");

            if (string.IsNullOrWhiteSpace(dto.ContactPhone))
                return Result<AssistanceRequestDto>.ValidationError("Contact phone is required");

            var validPriorities = new[] { "Low", "Medium", "High", "Critical" };
            if (!validPriorities.Contains(dto.Priority))
                return Result<AssistanceRequestDto>.ValidationError("Invalid priority value");

            return null;
        }


        public async Task<Result<List<AssistanceRequestDto>>> GetAllRequestsAsync()
        {
            try
            {
                var requests = await _requestRepository.GetAllAsync();
                var requestDtos = new List<AssistanceRequestDto>();

                foreach (var request in requests)
                {
                    var user = request.UserId.HasValue ? await _userRepository.GetByIdAsync(request.UserId.Value) : null;
                    requestDtos.Add(MapToDto(request, user));
                }

                return Result<List<AssistanceRequestDto>>.Success(requestDtos);
            }
            catch (Exception ex)
            {
                return Result<List<AssistanceRequestDto>>.Failure($"Error retrieving assistance requests: {ex.Message}");
            }
        }

        public async Task<Result<List<AssistanceRequestDto>>> GetUserRequestsAsync(Guid userId)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                    return Result<List<AssistanceRequestDto>>.NotFoundError("User not found");

                var requests = await _requestRepository.GetByUserIdAsync(userId);
                var requestDtos = requests.Select(r => MapToDto(r, user)).ToList();

                return Result<List<AssistanceRequestDto>>.Success(requestDtos);
            }
            catch (Exception ex)
            {
                return Result<List<AssistanceRequestDto>>.Failure($"Error retrieving user requests: {ex.Message}");
            }
        }

        public async Task<Result<AssistanceRequestDto>> GetRequestByIdAsync(int id)
        {
            try
            {
                var request = await _requestRepository.GetByIdAsync(id);
                if (request == null)
                    return Result<AssistanceRequestDto>.NotFoundError("Request not found");

                var user = request.UserId.HasValue ? await _userRepository.GetByIdAsync(request.UserId.Value) : null;
                var requestDto = MapToDto(request, user);

                return Result<AssistanceRequestDto>.Success(requestDto);
            }
            catch (Exception ex)
            {
                return Result<AssistanceRequestDto>.Failure($"Error retrieving request: {ex.Message}");
            }
        }

        public async Task<Result<AssistanceRequestDto>> UpdateRequestAsync(
         int id,
         UpdateAssistanceRequestDto updateDto,
         Guid userId)
        {
            try
            {
             
              

                // Get existing request
                var existingRequest = await _requestRepository.GetByIdAsync(id);
                if (existingRequest == null)
                    return Result<AssistanceRequestDto>.NotFoundError("Request not found");

                // Authorization check
                if (existingRequest.UserId != userId)
                    return Result<AssistanceRequestDto>.ValidationError("Unauthorized to update this request");

                // Business rule: Only pending requests can be modified
                if (existingRequest.Status != "Pending")
                    return Result<AssistanceRequestDto>.ValidationError("Only pending requests can be modified");

                // Validate references
                if (updateDto.LocationId.HasValue && updateDto.LocationId.Value != 0)
                {
                    if (!await _locationRepository.ExistsAsync(updateDto.LocationId.Value))
                        return Result<AssistanceRequestDto>.ValidationError("Invalid location reference");
                }

                if (updateDto.DisasterEventId.HasValue)
                {
                    if (!await _disasterEventRepository.ExistsAsync(updateDto.DisasterEventId.Value))
                        return Result<AssistanceRequestDto>.ValidationError("Invalid disaster event reference");
                }

                // Apply updates
                existingRequest.DisasterEventId = updateDto.DisasterEventId;
                existingRequest.LocationId = updateDto.LocationId == 0 ? null : updateDto.LocationId;
                existingRequest.SupportType = updateDto.SupportType;
                existingRequest.Quantity = updateDto.Quantity;
                existingRequest.Unit = updateDto.Unit;
                existingRequest.Description = updateDto.Description;
                existingRequest.Priority = updateDto.Priority;
                existingRequest.ContactName = updateDto.ContactName;
                existingRequest.Email = updateDto.Email;
                existingRequest.ContactPhone = updateDto.ContactPhone;
                existingRequest.DetailedAddress = updateDto.DetailedAddress;
                existingRequest.UpdatedAt = DateTime.UtcNow;

                // Persist changes
                await _requestRepository.UpdateAsync(existingRequest);

                // Return updated resource
                var updatedRequest = await _requestRepository.GetByIdAsync(id);
                var user = await _userRepository.GetByIdAsync(userId);

                return Result<AssistanceRequestDto>.Success(
                    MapToDto(updatedRequest, user),
                    "Request updated successfully");
            }
            catch (DbUpdateException dbEx) when (dbEx.InnerException is SqlException sqlEx && sqlEx.Number == 547)
            {
                _logger.LogError(dbEx, "Foreign key violation during update");
                return Result<AssistanceRequestDto>.ValidationError("Invalid reference to related entity");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating request");
                return Result<AssistanceRequestDto>.Failure("Error updating request");
            }
        }

        public async Task<Result<bool>> DeleteRequestAsync(int id, Guid userId)
        {
            try
            {
                var request = await _requestRepository.GetByIdAsync(id);
                if (request == null)
                    return Result<bool>.NotFoundError("Request not found");

                if (request.UserId != userId)
                    return Result<bool>.ValidationError("You can only delete your own requests");

                if (request.Status != "Pending")
                    return Result<bool>.ValidationError("Only pending requests can be deleted");

                await _requestRepository.DeleteAsync(id);
                return Result<bool>.Success(true, "Request deleted successfully");
            }
            catch (Exception ex)
            {
                return Result<bool>.Failure($"Error deleting request: {ex.Message}");
            }
        }

        // AssistanceRequestService.cs
        public async Task<Result<AssistanceRequestDto>> UpdateRequestStatusAsync(
            int id,
            UpdateRequestStatusDto statusDto,
            Guid adminId)
        {
            try
            {
                // Validate status
                var validStatuses = new[] { "Pending", "Approved", "InProgress", "Fulfilled", "Rejected" };
                if (!validStatuses.Contains(statusDto.Status))
                {
                    _logger.LogWarning("Invalid status value provided: {Status}", statusDto.Status);
                    return Result<AssistanceRequestDto>.ValidationError("Invalid status value. Allowed values: Pending, Approved, InProgress, Fulfilled, Rejected");
                }

                // Get request
                var request = await _requestRepository.GetByIdAsync(id);
                if (request == null)
                {
                    _logger.LogWarning("Request with ID {RequestId} not found", id);
                    return Result<AssistanceRequestDto>.NotFoundError("Request not found");
                }

                // Get admin user
                var admin = await _userRepository.GetByIdAsync(adminId);
                if (admin == null || admin.Role != "Admin")
                {
                    _logger.LogWarning("Unauthorized status update attempt by user {UserId}", adminId);
                    return Result<AssistanceRequestDto>.ValidationError("Only admins can update request status");
                }

                // Business rules for status transitions
                if (request.Status == "Fulfilled" && statusDto.Status != "Fulfilled")
                {
                    return Result<AssistanceRequestDto>.ValidationError("Cannot change status from Fulfilled");
                }

                if (request.Status == "Rejected" && statusDto.Status != "Rejected")
                {
                    return Result<AssistanceRequestDto>.ValidationError("Cannot change status from Rejected");
                }

                // Update status
                request.Status = statusDto.Status;
                request.UpdatedAt = DateTime.UtcNow;

                // Special handling for Fulfilled status
                if (statusDto.Status == "Fulfilled")
                {
                    request.FulfilledAt = DateTime.UtcNow;
                }

                await _requestRepository.UpdateAsync(request);

                // Notify the user about the status change
                if (request.UserId.HasValue)
                {
                    await _notificationService.NotifyUserForRequestUpdate(
                        request.UserId.Value,
                        request.Id,
                        request.SupportType,
                        statusDto.Status);
                }

                // Get updated request with related data
                var updatedRequest = await _requestRepository.GetByIdAsync(id);
                var user = request.UserId.HasValue ? await _userRepository.GetByIdAsync(request.UserId.Value) : null;
                var requestDto = MapToDto(updatedRequest, user);

                _logger.LogInformation("Request {RequestId} status updated to {Status} by admin {AdminId}",
                    id, statusDto.Status, adminId);

                return Result<AssistanceRequestDto>.Success(
                    requestDto,
                    $"Request status updated to {statusDto.Status}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating request status for request {RequestId}", id);
                return Result<AssistanceRequestDto>.Failure($"Error updating request status: {ex.Message}");
            }
        }

        public async Task<Result<List<AssistanceRequestDto>>> GetRequestsByDisasterAsync(int disasterEventId)
        {
            try
            {
                var requests = await _requestRepository.GetByDisasterEventAsync(disasterEventId);
                var requestDtos = new List<AssistanceRequestDto>();

                foreach (var request in requests)
                {
                    var user = request.UserId.HasValue ? await _userRepository.GetByIdAsync(request.UserId.Value) : null;
                    requestDtos.Add(MapToDto(request, user));
                }

                return Result<List<AssistanceRequestDto>>.Success(requestDtos);
            }
            catch (Exception ex)
            {
                return Result<List<AssistanceRequestDto>>.Failure($"Error retrieving requests by disaster: {ex.Message}");
            }
        }

        public async Task<Result<List<AssistanceRequestDto>>> GetRequestsByStatusAsync(string status)
        {
            try
            {
                // Validate status
                var validStatuses = new[] { "Pending", "Approved", "InProgress", "Fulfilled", "Rejected" };
                if (!validStatuses.Contains(status))
                    return Result<List<AssistanceRequestDto>>.ValidationError("Invalid status value");

                var requests = await _requestRepository.GetByStatusAsync(status);
                var requestDtos = new List<AssistanceRequestDto>();

                foreach (var request in requests)
                {
                    var user = request.UserId.HasValue ? await _userRepository.GetByIdAsync(request.UserId.Value) : null;
                    requestDtos.Add(MapToDto(request, user));
                }

                return Result<List<AssistanceRequestDto>>.Success(requestDtos);
            }
            catch (Exception ex)
            {
                return Result<List<AssistanceRequestDto>>.Failure($"Error retrieving requests by status: {ex.Message}");
            }
        }

        private AssistanceRequestDto MapToDto(AssistanceRequest request, User user)
        {
            return new AssistanceRequestDto
            {
                Id = request.Id,
                DisasterEventId = request.DisasterEventId,
                DisasterEventName = request.DisasterEvent?.Name,
                DisasterReportId = request.DisasterReportId,
                UserId = request.UserId,
                UserName = user?.Name,
                LocationId = request.LocationId,
                LocationName = request.Location?.Name,
                SupportType = request.SupportType,
                Quantity = request.Quantity,
                Unit = request.Unit,
                Description = request.Description,
                Priority = request.Priority,
                Status = request.Status,
                ContactName = request.ContactName,
                Email = request.Email,
                ContactPhone = request.ContactPhone,
                DetailedAddress = request.DetailedAddress,
                CreatedAt = request.CreatedAt,
                UpdatedAt = request.UpdatedAt,
                FulfilledAt = request.FulfilledAt
            };
        }
    }
}

using DisasterManagementSystem_Data.Models;
using DisasterManagementSystem_Data.Repositories;
using DisasterManagementSystem_Data.Repositories.Interfaces;
using DisasterManagementSystem_Services.Models;
using DisasterManagementSystem_Services.Models.AssistanceRequestDtos.DisasterManagementSystem_Service.Models.Dtos;
using DisasterManagementSystem_Services.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisasterManagementSystem_Services.Services.Implements
{
    public class RequestAssignmentService : IRequestAssignmentService
    {
        private readonly IRequestAssignmentRepository _assignmentRepository;
        private readonly IAssistanceRequestRepository _requestRepository;
        private readonly IReliefTeamRepository _reliefTeamRepository;
        private readonly IUserRepository _userRepository;
        private readonly INotificationService _notificationService;
        private readonly ILogger<RequestAssignmentService> _logger;

        public RequestAssignmentService(
            IRequestAssignmentRepository assignmentRepository,
            IAssistanceRequestRepository requestRepository,
            IReliefTeamRepository reliefTeamRepository,
            IUserRepository userRepository,
            INotificationService notificationService,
            ILogger<RequestAssignmentService> logger)
        {
            _assignmentRepository = assignmentRepository;
            _requestRepository = requestRepository;
            _reliefTeamRepository = reliefTeamRepository;
            _userRepository = userRepository;
            _notificationService = notificationService;
            _logger = logger;
        }
        public async Task<Result<List<RequestAssignmentDto>>> GetAllAssignmentsAsync()
        {
            try
            {
                var assignments = await _assignmentRepository.GetAllAsync();
                var dtos = new List<RequestAssignmentDto>();

                foreach (var assignment in assignments)
                {
                    var request = await _requestRepository.GetByIdAsync(assignment.AssistanceRequestId);
                    var reliefTeam = await _reliefTeamRepository.GetByIdAsync(assignment.ReliefTeamId);
                    var assignedBy = assignment.AssignedBy.HasValue
                        ? await _userRepository.GetByIdAsync(assignment.AssignedBy.Value)
                        : null;

                    dtos.Add(MapToDto(assignment, request, reliefTeam, assignedBy));
                }

                return Result<List<RequestAssignmentDto>>.Success(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all assignments");
                return Result<List<RequestAssignmentDto>>.Failure("Error getting all assignments");
            }
        }
        public async Task<Result<RequestAssignmentDto>> CreateAssignmentAsync(
            CreateRequestAssignmentDto dto, Guid adminId)
        {
            try
            {
                // Validate request exists
                var request = await _requestRepository.GetByIdAsync(dto.AssistanceRequestId);
                if (request == null)
                {
                    return Result<RequestAssignmentDto>.NotFoundError("Assistance request not found");
                }

                // ⏬ New check for cancelled status
                if (request.Status == "Rejected" || request.Status=="FullFilled" ||request.Status=="Pending")
                {
                    return Result<RequestAssignmentDto>.ValidationError($"Cannot assign a '{request.Status}' request");

                }

                // Validate relief team exists
                var reliefTeam = await _reliefTeamRepository.GetByIdAsync(dto.ReliefTeamId);
                if (reliefTeam == null)
                {
                    return Result<RequestAssignmentDto>.NotFoundError("Relief team not found");
                }

                // Validate admin exists
                var admin = await _userRepository.GetByIdAsync(adminId);

                // Check if user is neither SysAdmin nor DisasterManagementAdmin
                if (admin == null || (admin.Role != "SysAdmin" && admin.Role != "DisasterManagementAdmin"))
                {
                    return Result<RequestAssignmentDto>.ValidationError("Only admins can create assignments");
                }


                // Check if request is already assigned
                var existingAssignments = await _assignmentRepository.GetByRequestIdAsync(dto.AssistanceRequestId);
                if (existingAssignments.Any(a => a.Status != "Cancelled"))
                {
                    return Result<RequestAssignmentDto>.ValidationError("This request already has an active assignment");
                }

                // Create assignment
                var assignment = new RequestAssignment
                {
                    AssistanceRequestId = dto.AssistanceRequestId,
                    ReliefTeamId = dto.ReliefTeamId,
                    AssignedBy = adminId,
                    AssignedAt = DateTime.UtcNow,
                    Status = "Assigned",
                    Priority = dto.Priority,
                    Notes = dto.Notes
                };

                var createdAssignment = await _assignmentRepository.CreateAsync(assignment);

                // Update request status to "InProgress" if it was "Approved"
                if (request.Status == "Approved")
                {
                    request.Status = "InProgress";
                    await _requestRepository.UpdateAsync(request);
                }

                
            // Notify relief team members
            await _notificationService.NotifyReliefTeamAboutAssignment(
                dto.ReliefTeamId,
                dto.AssistanceRequestId,
                adminId);

                // Return created assignment
                return Result<RequestAssignmentDto>.Success(
                    MapToDto(createdAssignment, request, reliefTeam, admin),
                    "Request assigned successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating request assignment");
                return Result<RequestAssignmentDto>.Failure("Error creating request assignment");
            }
        }

        public async Task<Result<RequestAssignmentDto>> UpdateAssignmentStatusAsync(
            int id, UpdateAssignmentStatusDto dto, Guid userId)
        {
            try
            {
                // Validate assignment exists
                var assignment = await _assignmentRepository.GetByIdAsync(id);
                if (assignment == null)
                {
                    return Result<RequestAssignmentDto>.NotFoundError("Assignment not found");
                }

                // Get user making the update
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    return Result<RequestAssignmentDto>.NotFoundError("User not found");
                }

                // Validate status transition
                if (!IsValidStatusTransition(assignment.Status, dto.Status))
                {
                    return Result<RequestAssignmentDto>.ValidationError(
                        $"Invalid status transition from {assignment.Status} to {dto.Status}");
                }

                // Update status
                var success = await _assignmentRepository.UpdateStatusAsync(id, dto.Status, userId);
                if (!success)
                {
                    return Result<RequestAssignmentDto>.Failure("Failed to update assignment status");
                }

                // If assignment is completed, update request status if needed
                if (dto.Status == "Done")
                {
                    var assistanceRequest = await _requestRepository.GetByIdAsync(assignment.AssistanceRequestId);
                    if (assistanceRequest != null && assistanceRequest.Status == "InProgress")
                    {
                        assistanceRequest.Status = "Fulfilled";
                        assistanceRequest.FulfilledAt = DateTime.UtcNow;
                        await _requestRepository.UpdateAsync(assistanceRequest);
                    }
                }

                // Get updated assignment with related data
                var updatedAssignment = await _assignmentRepository.GetByIdAsync(id);
                var reliefTeam = await _reliefTeamRepository.GetByIdAsync(updatedAssignment.ReliefTeamId);
                var request = await _requestRepository.GetByIdAsync(updatedAssignment.AssistanceRequestId);
                var assignedBy = await _userRepository.GetByIdAsync(updatedAssignment.AssignedBy.Value);

                return Result<RequestAssignmentDto>.Success(
                    MapToDto(updatedAssignment, request, reliefTeam, assignedBy),
                    "Assignment status updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating assignment status");
                return Result<RequestAssignmentDto>.Failure("Error updating assignment status");
            }
        }

        public async Task<Result<List<RequestAssignmentDto>>> GetAssignmentsByRequestAsync(int requestId)
        {
            try
            {
                var assignments = await _assignmentRepository.GetByRequestIdAsync(requestId);
                var dtos = new List<RequestAssignmentDto>();

                foreach (var assignment in assignments)
                {
                    var request = await _requestRepository.GetByIdAsync(assignment.AssistanceRequestId);
                    var reliefTeam = await _reliefTeamRepository.GetByIdAsync(assignment.ReliefTeamId);
                    var assignedBy = assignment.AssignedBy.HasValue
                        ? await _userRepository.GetByIdAsync(assignment.AssignedBy.Value)
                        : null;

                    dtos.Add(MapToDto(assignment, request, reliefTeam, assignedBy));
                }

                return Result<List<RequestAssignmentDto>>.Success(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting assignments by request");
                return Result<List<RequestAssignmentDto>>.Failure("Error getting assignments by request");
            }
        }

        public async Task<Result<List<RequestAssignmentDto>>> GetAssignmentsByReliefTeamAsync(int reliefTeamId)
        {
            try
            {
                var assignments = await _assignmentRepository.GetByReliefTeamIdAsync(reliefTeamId);
                var dtos = new List<RequestAssignmentDto>();

                foreach (var assignment in assignments)
                {
                    var request = await _requestRepository.GetByIdAsync(assignment.AssistanceRequestId);
                    var reliefTeam = await _reliefTeamRepository.GetByIdAsync(assignment.ReliefTeamId);
                    var assignedBy = assignment.AssignedBy.HasValue
                        ? await _userRepository.GetByIdAsync(assignment.AssignedBy.Value)
                        : null;

                    dtos.Add(MapToDto(assignment, request, reliefTeam, assignedBy));
                }

                return Result<List<RequestAssignmentDto>>.Success(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting assignments by relief team");
                return Result<List<RequestAssignmentDto>>.Failure("Error getting assignments by relief team");
            }
        }

        public async Task<Result<RequestAssignmentDto>> GetAssignmentByIdAsync(int id)
        {
            try
            {
                var assignment = await _assignmentRepository.GetByIdAsync(id);
                if (assignment == null)
                {
                    return Result<RequestAssignmentDto>.NotFoundError("Assignment not found");
                }

                var request = await _requestRepository.GetByIdAsync(assignment.AssistanceRequestId);
                var reliefTeam = await _reliefTeamRepository.GetByIdAsync(assignment.ReliefTeamId);
                var assignedBy = assignment.AssignedBy.HasValue
                    ? await _userRepository.GetByIdAsync(assignment.AssignedBy.Value)
                    : null;

                return Result<RequestAssignmentDto>.Success(
                    MapToDto(assignment, request, reliefTeam, assignedBy));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting assignment by ID");
                return Result<RequestAssignmentDto>.Failure("Error getting assignment by ID");
            }
        }

        public async Task<Result<List<RequestAssignmentDto>>> GetAssignmentsByUserAsync(Guid userId)
        {
            try
            {
                // Get all teams where user is the creator
                var userTeams = await _reliefTeamRepository.GetTeamsByUserIdAsync(userId);
                if (!userTeams.Any())
                {
                    return Result<List<RequestAssignmentDto>>.Success(new List<RequestAssignmentDto>());
                }

                // Get all assignments for user's teams
                var assignments = new List<RequestAssignment>();
                foreach (var team in userTeams)
                {
                    var teamAssignments = await _assignmentRepository.GetByReliefTeamIdAsync(team.Id);
                    assignments.AddRange(teamAssignments);
                }

                // Map to DTOs
                var dtos = new List<RequestAssignmentDto>();
                foreach (var assignment in assignments)
                {
                    var request = await _requestRepository.GetByIdAsync(assignment.AssistanceRequestId);
                    var reliefTeam = userTeams.FirstOrDefault(rt => rt.Id == assignment.ReliefTeamId);
                    var assignedBy = assignment.AssignedBy.HasValue
                        ? await _userRepository.GetByIdAsync(assignment.AssignedBy.Value)
                        : null;

                    dtos.Add(MapToDto(assignment, request, reliefTeam, assignedBy));
                }

                return Result<List<RequestAssignmentDto>>.Success(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting assignments by user");
                return Result<List<RequestAssignmentDto>>.Failure("Error getting assignments by user");
            }
        }

        private bool IsValidStatusTransition(string currentStatus, string newStatus)
        {
            var validTransitions = new Dictionary<string, List<string>>
            {
                { "Assigned", new List<string> { "InProgress", "Cancelled" } },
                { "InProgress", new List<string> { "Done", "Cancelled" } },
                { "Done", new List<string>() },
                { "Cancelled", new List<string>() }
            };

            return validTransitions[currentStatus].Contains(newStatus);
        }

        private RequestAssignmentDto MapToDto(
            RequestAssignment assignment,
            AssistanceRequest request,
            ReliefTeam reliefTeam,
            User assignedBy)
        {
            return new RequestAssignmentDto
            {
                Id = assignment.Id,
                AssistanceRequestId = assignment.AssistanceRequestId,
                RequestDetails = request != null ? new AssistanceRequestDto
                {
                    Id = request.Id,
                    DisasterEventName=request.DisasterEvent.Name,
                    SupportType = request.SupportType,
                    Quantity = request.Quantity,
                    Unit = request.Unit,
                    Priority = request.Priority,
                    Status = request.Status,
                    Email=request.Email,
                    Description = request.Description,
                    ContactName = request.ContactName,
                    ContactPhone = request.ContactPhone,
                    DetailedAddress = request.DetailedAddress
                } : null,
                ReliefTeamId = assignment.ReliefTeamId,
                ReliefTeamName = reliefTeam?.Name,
                AssignedById = assignment.AssignedBy,
                AssignedByName = assignedBy?.Name,
                AssignedAt = assignment.AssignedAt,
                Status = assignment.Status,
                Priority = assignment.Priority,
                Notes = assignment.Notes,
                CompletedAt = assignment.CompletedAt,
                LastUpdatedById = assignment.LastUpdatedBy,
                UpdatedAt = assignment.UpdatedAt
            };
        }
    }
}

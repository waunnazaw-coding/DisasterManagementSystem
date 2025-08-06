using DisasterManagementSystem_Data.Models;
using DisasterManagementSystem_Data.Repositories;
using DisasterManagementSystem_Data.Repositories.Implements;
using DisasterManagementSystem_Data.Repositories.Interfaces;
using DisasterManagementSystem_Services.Hubs;
using DisasterManagementSystem_Services.Models.NotificationDto.cs;
using DisasterManagementSystem_Services.Services.Interfaces;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisasterManagementSystem_Services.Services.Implements
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepo;
        private readonly IUserRepository _userRepo;
        private readonly IAssistanceRequestRepository _requestRepository;
        private readonly IReliefTeamRepository _reliefTeamRepository;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly ILogger<NotificationService> _logger;
        private readonly AppDbContext _context;

        public NotificationService(
            INotificationRepository notificationRepo,
            IUserRepository userRepo,
            IAssistanceRequestRepository requestRepository,
            IReliefTeamRepository reliefTeamRepository,
            IHubContext<NotificationHub> hubContext,
            ILogger<NotificationService> logger,
            AppDbContext context)
        {
            _notificationRepo = notificationRepo ?? throw new ArgumentNullException(nameof(notificationRepo));
            _userRepo = userRepo ?? throw new ArgumentNullException(nameof(userRepo));
            _requestRepository = requestRepository ?? throw new ArgumentNullException(nameof(requestRepository));
            _reliefTeamRepository = reliefTeamRepository ?? throw new ArgumentNullException(nameof(reliefTeamRepository));
            _hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }
        public async Task<NotificationDto> CreateNotificationAsync(CreateNotificationDto dto)
        {
            var notification = new Notification
            {
                UserId = dto.UserId,
                Message = dto.Message,
                Type = dto.Type,
                RelatedEntityId = dto.RelatedEntityId,
                Status = dto.Status,
                CreatedAt = DateTime.UtcNow
            };

            var created = await _notificationRepo.AddAsync(notification);

            // Send real-time notification
            await _hubContext.Clients.User(dto.UserId.ToString())
                .SendAsync("ReceiveNotification", MapToDto(created));

            return MapToDto(created);
        }
        public async Task NotifyAdminsForNewDonation(Donation donation)
        {
            try
            {
                var admins = await _userRepo.GetUsersByRoleAsync("Admin");
                var donorName = donation.DonorUser?.Name ?? "Anonymous";
                var message = $"New donation received: {donation.Type} from {donorName}";

                foreach (var admin in admins)
                {
                    await CreateNotificationAsync(new CreateNotificationDto
                    {
                        UserId = admin.Id,
                        Message = message,
                        Type = "Donation",
                        RelatedEntityId = donation.Id,
                        Status = "New"
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error notifying admins about new donation");
            }
        }
        public async Task NotifyDonorAboutStatusChange(Donation donation)
        {
            try
            {
                if (!donation.DonorUserId.HasValue) return;

                var message = $"Your donation ({donation.Type}) has been {donation.Status.ToLower()}";

                await CreateNotificationAsync(new CreateNotificationDto
                {
                    UserId = donation.DonorUserId.Value,
                    Message = message,
                    Type = "Donation",
                    RelatedEntityId = donation.Id,
                    Status = donation.Status
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error notifying donor about status change");
            }
        }

        public async Task NotifyAdminsForNewReport(Guid userId, int reportId, string reportTitle)
        {
            var admins = await _userRepo.GetUsersByRoleAsync("Admin");
            var message = $"New disaster report submitted: {reportTitle}";

            foreach (var admin in admins)
            {
                await CreateNotificationAsync(new CreateNotificationDto
                {
                    UserId = admin.Id,
                    Message = message,
                    Type = "Report",
                    RelatedEntityId = reportId,
                    Status = "Pending"
                });
            }
        }

        public async Task NotifyUserForReportUpdate(Guid userId, int reportId, string reportTitle, string status)
        {
            var message = $"Your report '{reportTitle}' has been {status.ToLower()}";

            await CreateNotificationAsync(new CreateNotificationDto
            {
                UserId = userId,
                Message = message,
                Type = "Report",
                RelatedEntityId = reportId,
                Status = status
            });
        }

        public async Task NotifyAdminsForNewRequest(Guid userId, int requestId, string requestType)
        {
            try
            {
                var admins = await _userRepo.GetUsersByRoleAsync("Admin");
                var user = await _userRepo.GetByIdAsync(userId);
                var message = $"New assistance request for {requestType} from {user?.Name}";

                foreach (var admin in admins)
                {
                    var notification = await CreateNotificationAsync(new CreateNotificationDto
                    {
                        UserId = admin.Id,
                        Message = message,
                        Type = "Request",
                        RelatedEntityId = requestId,
                        Status = "Pending"
                    });

                    // Send real-time notification to admin group
                    await _hubContext.Clients.Group("Admins")
                        .SendAsync("ReceiveAdminNotification", notification);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error notifying admins for new request");
            }
        }

        public async Task NotifyUserForRequestUpdate(Guid userId, int requestId, string requestType, string status)
        {
            try
            {
                var message = $"Your request for {requestType} has been {status.ToLower()}";

                var notification = await CreateNotificationAsync(new CreateNotificationDto
                {
                    UserId = userId,
                    Message = message,
                    Type = "Request",
                    RelatedEntityId = requestId,
                    Status = status
                });

                // Send real-time notification to user
                await _hubContext.Clients.User(userId.ToString())
                    .SendAsync("ReceiveNotification", notification);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error notifying user for request update");
            }
        }


        public async Task<List<NotificationDto>> GetUserNotificationsAsync(Guid userId)
        {
            var notifications = await _notificationRepo.GetByUserIdAsync(userId);
            return notifications.Select(MapToDto).ToList();
        }

        public async Task<NotificationDto> MarkAsReadAsync(int notificationId)
        {
            var notification = await _notificationRepo.MarkAsReadAsync(notificationId);
            return notification != null ? MapToDto(notification) : null;
        }
        public async Task NotifyReliefTeamAboutAssignment(int reliefTeamId, int requestId, Guid assignedByUserId)
        {
            try
            {
                // Get request details
                var request = await _requestRepository.GetByIdAsync(requestId);
                if (request == null)
                {
                    _logger.LogError("Request {RequestId} not found", requestId);
                    return;
                }

                // Get assigner details
                var assigner = await _userRepo.GetByIdAsync(assignedByUserId);
                var assignerName = assigner?.Name ?? "System Admin";

                // Get relief team details
                var reliefTeam = await _reliefTeamRepository.GetByIdAsync(reliefTeamId);
                if (reliefTeam == null)
                {
                    _logger.LogError("Relief team {ReliefTeamId} not found", reliefTeamId);
                    return;
                }

                if (!reliefTeam.UserId.HasValue)
                {
                    _logger.LogWarning("Relief team {ReliefTeamId} has no associated user", reliefTeamId);
                    return;
                }

                var message = $"New request assigned to your team: {request.SupportType} " +
                             $"(Priority: {request.Priority}) by {assignerName}";

                // Create notification for the relief team's user
                try
                {
                    var notification = new Notification
                    {
                        UserId = reliefTeam.UserId.Value, // Send to relief team's user
                        Message = message,
                        Type = "TeamAssignment",
                        RelatedEntityId = requestId,
                        Status = "New",
                        CreatedAt = DateTime.UtcNow
                    };

                    await _notificationRepo.AddAsync(notification);

                    // Send real-time notification to relief team's user
                    await _hubContext.Clients.User(reliefTeam.UserId.Value.ToString())
                        .SendAsync("ReceiveNotification", new
                        {
                            notification.Id,
                            notification.Message,
                            notification.Type,
                            notification.RelatedEntityId,
                            notification.CreatedAt
                        });

                    _logger.LogInformation("Notification created for relief team user {UserId}", reliefTeam.UserId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error notifying relief team user {UserId}", reliefTeam.UserId);
                }

                // Optional: Also notify individual team members if needed
                //var teamMembers = await _reliefTeamRepository.GetTeamMembersAsync(reliefTeamId);
                //if (teamMembers != null && teamMembers.Any())
                //{
                //    foreach (var member in teamMembers)
                //    {
                //        try
                //        {
                //            // Skip if this is the relief team's main user (already notified)
                //            if (member.Id == reliefTeam.UserId) continue;

                //            var memberNotification = new Notification
                //            {
                //                UserId = member.Id,
                //                Message = message,
                //                Type = "TeamAssignment",
                //                RelatedEntityId = requestId,
                //                Status = "New",
                //                CreatedAt = DateTime.UtcNow
                //            };

                //            await _notificationRepo.AddAsync(memberNotification);

                //            await _hubContext.Clients.User(member.Id.ToString())
                //                .SendAsync("ReceiveNotification", new
                //                {
                //                    memberNotification.Id,
                //                    memberNotification.Message,
                //                    memberNotification.Type,
                //                    memberNotification.RelatedEntityId,
                //                    memberNotification.CreatedAt
                //                });
                //        }
                //        catch (Exception ex)
                //        {
                //            _logger.LogError(ex, "Error notifying team member {UserId}", member.Id);
                //        }
                //    }
                //}
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error notifying relief team about assignment");
            }
        }
        public async Task<int> GetUnreadCountAsync(Guid userId)
        {
            return await _notificationRepo.GetUnreadCountAsync(userId);
        }

        private NotificationDto MapToDto(Notification notification) => new()
        {
            Id = notification.Id,
            UserId = notification.UserId,
            Message = notification.Message,
            IsRead = notification.IsRead,
            CreatedAt = notification.CreatedAt,
            Type = notification.Type,
            RelatedEntityId = notification.RelatedEntityId,
            Status = notification.Status
        };
    }
}

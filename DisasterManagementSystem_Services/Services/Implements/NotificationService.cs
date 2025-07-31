using DisasterManagementSystem_Data.Models;
using DisasterManagementSystem_Data.Repositories.Interfaces;
using DisasterManagementSystem_Services.Hubs;
using DisasterManagementSystem_Services.Models.NotificationDto.cs;
using DisasterManagementSystem_Services.Services.Interfaces;
using Microsoft.AspNetCore.SignalR;
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
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(
            INotificationRepository notificationRepo,
            IUserRepository userRepo,
            IHubContext<NotificationHub> hubContext,
            ILogger<NotificationService> logger)
        {
            _notificationRepo = notificationRepo;
            _userRepo = userRepo;
            _hubContext = hubContext;
            _logger = logger;
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

using DisasterManagementSystem_Data.Models;
using DisasterManagementSystem_Services.Models.NotificationDto.cs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisasterManagementSystem_Services.Services.Interfaces
{
    public interface INotificationService
    {
        Task<NotificationDto> CreateNotificationAsync(CreateNotificationDto dto);
        Task<List<NotificationDto>> GetUserNotificationsAsync(Guid userId);
        Task<NotificationDto> MarkAsReadAsync(int notificationId);
        Task<int> GetUnreadCountAsync(Guid userId);
        Task NotifyAdminsForNewReport(Guid? userId, int reportId, string reportTitle);
        Task NotifyUserForReportUpdate(Guid userId, int reportId, string reportTitle, string status);
        Task NotifyAdminsForNewRequest(Guid userId, int requestId, string requestType);
        Task NotifyUserForRequestUpdate(Guid userId, int requestId, string requestType, string status);
        Task NotifyAdminsForNewDonation(Donation donation);
        Task NotifyDonorAboutStatusChange(Donation donation);

        Task NotifyReliefTeamAboutAssignment(int reliefTeamId, int requestId, Guid assignedByUserId);
    }
}

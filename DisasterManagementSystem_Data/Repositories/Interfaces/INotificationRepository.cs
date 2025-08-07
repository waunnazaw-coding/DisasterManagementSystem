using DisasterManagementSystem_Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisasterManagementSystem_Data.Repositories.Interfaces
{
    public interface INotificationRepository
    {
        Task<Notification> AddAsync(Notification notification);
        Task<List<Notification>> GetByUserIdAsync(Guid userId);
        Task<Notification> MarkAsReadAsync(int notificationId);
        Task<int> GetUnreadCountAsync(Guid userId);
        Task<List<User>> GetUsersByRoleAsync(string role);
    }
}

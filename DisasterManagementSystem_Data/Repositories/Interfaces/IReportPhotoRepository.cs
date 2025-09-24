using DisasterManagementSystem_Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisasterManagementSystem_Data.Repositories.Interfaces
{
    public interface IReportPhotoRepository
    {
        Task<ReportPhoto> AddAsync(ReportPhoto photo);
        Task<ReportPhoto?> UpdateAsync(ReportPhoto photo);
        Task<List<ReportPhoto>> GetByReportIdAsync(int reportId);
        Task<List<ReportPhoto>> GetByEventIdAsync(int eventId);
        Task<ReportPhoto?> GetByIdAsync(int photoId);
        Task<bool> DeleteAsync(int photoId);
        Task<List<ReportPhoto>> GetByActivityIdAsync(int activityId);
    }
}

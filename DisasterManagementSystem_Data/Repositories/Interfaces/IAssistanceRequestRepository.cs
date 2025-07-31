using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DisasterManagementSystem_Data.Models;

namespace DisasterManagementSystem_Data.Repositories
{
    public interface IAssistanceRequestRepository
    {
        Task<AssistanceRequest> AddAsync(AssistanceRequest request);
        Task<IEnumerable<AssistanceRequest>> GetAllAsync();
        Task<AssistanceRequest> GetByIdAsync(int id);
        Task<bool> UpdateAsync(AssistanceRequest request);
        Task<bool> DeleteAsync(int id);
        Task<IEnumerable<AssistanceRequest>> GetByUserIdAsync(Guid userId);
        Task<IEnumerable<AssistanceRequest>> GetByDisasterEventAsync(int disasterEventId);
        Task<IEnumerable<AssistanceRequest>> GetByStatusAsync(string status);
        Task LoadRelatedEntitiesAsync(AssistanceRequest request);
        Task<DisasterEvent?> GetDisasterEventAsync(int? disasterEventId);
    }
}


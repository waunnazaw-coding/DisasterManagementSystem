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
        Task<IEnumerable<AssistanceRequest>> GetAllAsync();
        Task<AssistanceRequest?> GetByIdAsync(int id);
        Task AddAsync(AssistanceRequest request);
        Task UpdateAsync(AssistanceRequest request);
        Task DeleteAsync(int id);
        Task<bool> SaveChangesAsync();
    }
}


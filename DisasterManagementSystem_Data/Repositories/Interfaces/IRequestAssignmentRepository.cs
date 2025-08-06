using DisasterManagementSystem_Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisasterManagementSystem_Data.Repositories.Interfaces
{
    public interface IRequestAssignmentRepository
    {
        Task<RequestAssignment> GetByIdAsync(int id);
        Task<IEnumerable<RequestAssignment>> GetAllAsync();
        Task<IEnumerable<RequestAssignment>> GetByRequestIdAsync(int requestId);
        Task<IEnumerable<RequestAssignment>> GetByReliefTeamIdAsync(int reliefTeamId);
        Task<RequestAssignment> CreateAsync(RequestAssignment assignment);
        Task<bool> UpdateAsync(RequestAssignment assignment);
        Task<bool> DeleteAsync(int id);
        Task<bool> UpdateStatusAsync(int id, string status, Guid updatedBy);
    }
}

using DisasterManagementSystem_Services.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisasterManagementSystem_Services.Services.Interfaces
{
    public interface IRequestAssignmentService
    {
        Task<Result<RequestAssignmentDto>> CreateAssignmentAsync(CreateRequestAssignmentDto dto, Guid adminId);
        Task<Result<RequestAssignmentDto>> UpdateAssignmentStatusAsync(int id, UpdateAssignmentStatusDto dto, Guid userId);
        Task<Result<List<RequestAssignmentDto>>> GetAssignmentsByRequestAsync(int requestId);
        Task<Result<List<RequestAssignmentDto>>> GetAssignmentsByReliefTeamAsync(int reliefTeamId);
        Task<Result<RequestAssignmentDto>> GetAssignmentByIdAsync(int id);

        Task<Result<List<RequestAssignmentDto>>> GetAllAssignmentsAsync();
    }
}

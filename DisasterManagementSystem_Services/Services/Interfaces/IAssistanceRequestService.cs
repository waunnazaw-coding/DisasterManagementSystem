using DisasterManagementSystem_Services.Models;
using DisasterManagementSystem_Services.Models.AssistanceRequestDtos;
using DisasterManagementSystem_Services.Models.AssistanceRequestDtos.DisasterManagementSystem_Service.Models.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisasterManagementSystem_Services.Services.Interfaces
{
    public interface IAssistanceRequestService
    {
        Task<Result<AssistanceRequestDto>> CreateRequestAsync(CreateAssistanceRequestDto requestDto, Guid userId);
        Task<Result<List<AssistanceRequestDto>>> GetAllRequestsAsync();
        Task<Result<List<AssistanceRequestDto>>> GetUserRequestsAsync(Guid userId);
        Task<Result<AssistanceRequestDto>> GetRequestByIdAsync(int id, bool includeAssignments = false);
        Task<Result<AssistanceRequestDto>> UpdateRequestAsync(int id, UpdateAssistanceRequestDto requestDto, Guid userId);
        Task<Result<bool>> DeleteRequestAsync(int id, Guid userId);
        Task<Result<AssistanceRequestDto>> UpdateRequestStatusAsync(int id, UpdateRequestStatusDto statusDto, Guid adminId);
        Task<Result<List<AssistanceRequestDto>>> GetRequestsByDisasterAsync(int disasterEventId);
        Task<Result<List<AssistanceRequestDto>>> GetRequestsByStatusAsync(string status);
        Task<Result<RequestStatsDto>> GetRequestStatsAsync();

        Task<Result<List<AssistanceRequestDto>>> GetAllRequestsWithAssignmentsAsync();
    }
}

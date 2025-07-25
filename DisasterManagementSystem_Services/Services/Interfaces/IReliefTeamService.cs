using DisasterManagementSystem_Services.Models;
using DisasterManagementSystem_Services.Models.ReliefTeamDtos;

namespace DisasterManagementSystem_Services.Services.Interfaces;

public interface IReliefTeamService
{
    Task<Result<ReliefTeamResponseDTO>>  CreateReliefTeamAndInviteAsync(CreateReliefTeamRequestDto dto);
}
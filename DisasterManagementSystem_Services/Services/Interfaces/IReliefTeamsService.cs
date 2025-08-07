using DisasterManagementSystem_Services.Models;
using DisasterManagementSystem_Services.Models.ReliefTeamDtos;

namespace DisasterManagementSystem_Services.Services.Interfaces;

public interface IReliefTeamsService
{
    Task<Result<ReliefTeamResponseDTO>>  CreateReliefTeamAndInviteAsync(CreateReliefTeamRequestDto dto);
    Task<Result<List<ReliefTeamResponseDTO>>> GetAllAsync();

    Task<Result<ReliefTeamResponseDTO>> GetByIdAsync(int id);

    Task<Result<OperationResponseDto>> UpdateAsync(int id, UpdateReliefTeamRequestDto dto);

    Task<Result<OperationResponseDto>> DeleteAsync(int id);
}
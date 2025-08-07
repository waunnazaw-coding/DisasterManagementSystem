using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisasterManagementSystem_Services.Services.Implements
{
    using DisasterManagementSystem_Data.Models;
    using DisasterManagementSystem_Data.Repositories.Interfaces;
    
    using global::DisasterManagementSystem_Services.Models;
    using global::DisasterManagementSystem_Services.Services.Interfaces;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;

    namespace DisasterManagementSystem_Services.Services.Implements
    {
        public class ReliefTeamService : IReliefTeamService
        {
            private readonly IReliefTeamRepository _teamRepository;
            private readonly IlocationRepository _locationRepository;
            private readonly ILogger<ReliefTeamService> _logger;

            public ReliefTeamService(
                IReliefTeamRepository teamRepository,
                IlocationRepository locationRepository,
                ILogger<ReliefTeamService> logger)
            {
                _teamRepository = teamRepository;
                _locationRepository = locationRepository;
                _logger = logger;
            }

            public async Task<Result<ReliefTeamDto>> CreateTeamAsync(CreateReliefTeamDto dto)
            {
                try
                {
                    var team = new ReliefTeam
                    {
                        Name = dto.Name,
                        ContactInfo = dto.ContactInfo,
                        LocationId = dto.LocationId,
                        Address = dto.Address,
                        Status = "Active",
                        TeamLeaderName = dto.TeamLeaderName,
                        Email = dto.Email,
                        Phone = dto.Phone,
                        NumberOfMembers = dto.NumberOfMembers,
                        Specialization = dto.Specialization,
                        EstablishedDate = DateOnly.FromDateTime(DateTime.UtcNow) // ✅ correct

                    };

                    var createdTeam = await _teamRepository.CreateAsync(team);
                    return Result<ReliefTeamDto>.Success(MapToDto(createdTeam), "Team created successfully");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating relief team");
                    return Result<ReliefTeamDto>.Failure("Error creating relief team");
                }
            }

            public async Task<Result<List<ReliefTeamDto>>> GetAllTeamsAsync()
            {
                try
                {
                    var teams = await _teamRepository.GetAllAsync();
                    var dtos = teams.Select(MapToDto).ToList();
                    return Result<List<ReliefTeamDto>>.Success(dtos);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error getting relief teams");
                    return Result<List<ReliefTeamDto>>.Failure("Error getting relief teams");
                }
            }

            public async Task<Result<ReliefTeamDto>> GetTeamByIdAsync(int id)
            {
                try
                {
                    var team = await _teamRepository.GetByIdAsync(id);
                    if (team == null)
                        return Result<ReliefTeamDto>.NotFoundError("Team not found");

                    return Result<ReliefTeamDto>.Success(MapToDto(team));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error getting relief team");
                    return Result<ReliefTeamDto>.Failure("Error getting relief team");
                }
            }

            public async Task<Result<ReliefTeamDto>> UpdateTeamAsync(int id, UpdateReliefTeamDto dto)
            {
                try
                {
                    var team = await _teamRepository.GetByIdAsync(id);
                    if (team == null)
                        return Result<ReliefTeamDto>.NotFoundError("Team not found");

                    team.Name = dto.Name;
                    team.ContactInfo = dto.ContactInfo;
                    team.LocationId = dto.LocationId;
                    team.Address = dto.Address;
                    team.Status = dto.Status;
                    team.TeamLeaderName = dto.TeamLeaderName;
                    team.Email = dto.Email;
                    team.Phone = dto.Phone;
                    team.NumberOfMembers = dto.NumberOfMembers;
                    team.Specialization = dto.Specialization;

                    await _teamRepository.UpdateAsync(team);
                    return Result<ReliefTeamDto>.Success(MapToDto(team), "Team updated successfully");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating relief team");
                    return Result<ReliefTeamDto>.Failure("Error updating relief team");
                }
            }

            public async Task<Result<bool>> DeleteTeamAsync(int id)
            {
                try
                {
                    var success = await _teamRepository.DeleteAsync(id);
                    if (!success)
                        return Result<bool>.NotFoundError("Team not found");

                    return Result<bool>.Success(true, "Team deactivated successfully");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error deleting relief team");
                    return Result<bool>.Failure("Error deleting relief team");
                }
            }

            private ReliefTeamDto MapToDto(ReliefTeam team)
            {
                return new ReliefTeamDto
                {
                    Id = team.Id,
                    Name = team.Name,
                    ContactInfo = team.ContactInfo,
                    LocationId = team.LocationId,
                    LocationName = team.Location?.Name,
                    Address = team.Address,
                    Status = team.Status,
                    TeamLeaderName = team.TeamLeaderName,
                    Email = team.Email,
                    Phone = team.Phone,
                    NumberOfMembers = team.NumberOfMembers,
                    Specialization = team.Specialization,
                    EstablishedDate = team.EstablishedDate
                };
            }

            public async Task<Result<ReliefTeam>> GetTeamByUserIdAsync(Guid userId)
            {
                var team = await _teamRepository.GetByUserIdAsync(userId);

                if (team == null)
                {
                    return Result<ReliefTeam>.NotFoundError("No relief team found for this user");
                }

                return Result<ReliefTeam>.Success(team);
            }

        }
    }
}

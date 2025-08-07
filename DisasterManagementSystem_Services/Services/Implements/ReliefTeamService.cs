using DisasterManagementSystem_Data.Models;
using DisasterManagementSystem_Data.Repositories.Interfaces;
using DisasterManagementSystem_Services.Models;
using DisasterManagementSystem_Services.Models.ReliefTeamDtos;
using System.Threading.Tasks;
using DisasterManagementSystem_Data.Repositories;
using DisasterManagementSystem_Services.Services.Interfaces;

public class ReliefTeamService : IReliefTeamService
{
    private readonly IUserRepository _userRepository;
    private readonly IReliefTeamRepository _reliefTeamRepository;
    private readonly IUserReliefTeamRepository _userReliefTeamRepository;
    private readonly IJwtService _jwtService;
    private readonly IEmailSenderService _emailSender;

    public ReliefTeamService(
        IUserRepository userRepository,
        IReliefTeamRepository reliefTeamRepository,
        IUserReliefTeamRepository userReliefTeamRepository,
        IJwtService jwtService,
        IEmailSenderService emailSender)
    {
        _userRepository = userRepository;
        _reliefTeamRepository = reliefTeamRepository;
        _userReliefTeamRepository = userReliefTeamRepository;
        _jwtService = jwtService;
        _emailSender = emailSender;
    }

    public async Task<Result<ReliefTeamResponseDTO>> CreateReliefTeamAndInviteAsync(CreateReliefTeamRequestDto dto)
    {
        var team = new ReliefTeam
        {
            Name = dto.Name,
            ContactInfo = dto.ContactInfo,
            LocationId = dto.LocationId,
            Address = dto.Address,
            Status = dto.Status ?? "Active",
            TeamLeaderName = dto.TeamLeaderName ?? dto.Email,
            SocialMediaUrl = dto.SocialMediaURL,
            Email = dto.Email,
            Phone = dto.Phone,
            NumberOfMembers = dto.NumberOfMembers,
            Specialization = dto.Specialization,
            EquipmentDetails = dto.EquipmentDetails,
            EstablishedDate = dto.EstablishedDate,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        try
        {
            await _reliefTeamRepository.AddAsync(team);
            await _reliefTeamRepository.SaveChangesAsync();

            // Get or create user
            var user = await _userRepository.GetByEmailAsync(dto.Email);
            if (user == null)
            {
                user = new User
                {
                    Name = dto.TeamLeaderName ?? dto.Email,
                    Email = dto.Email,
                    Role = "ReliefTeam",
                    Status = "Active",
                    CreatedAt = DateTime.UtcNow,
                    PasswordHash = null,
                    AuthProvider = null
                };
                await _userRepository.AddAsync(user);
            }
            else if (user.Role != "ReliefTeam")
            {
                user.Role = "ReliefTeam";
                user.Status = "Active";
                await _userRepository.UpdateAsync(user);
            }

            // Link user with relief team
            var existingLink = await _userReliefTeamRepository.FindAsync(user.Id, team.Id);
            if (existingLink == null)
            {
                await _userReliefTeamRepository.AddAsync(new UserReliefTeam
                {
                    UserId = user.Id,
                    ReliefTeamId = team.Id
                });
                await _userReliefTeamRepository.SaveChangesAsync();
            }

            // Generate reset/set password token (valid 24h)
            var token = _jwtService.GenerateToken(user.Id, user.Role, TimeSpan.FromHours(24));
            var resetUrl = $"http://localhost:5173/reset-password?token={token}";

            var subject = "Relief Team Invitation - Set Your Password";
            var body = $@"
                <p>Hello {user.Name},</p>
                <p>You have been added as a member of the relief team '{team.Name}'.</p>
                <p>Please <a href='{resetUrl}'>click here</a> to set your password and access your team panel.</p>
                <p>This link expires in 24 hours.</p>";

            await _emailSender.SendEmailAsync(user.Email, subject, body);

            var responseDto = new ReliefTeamResponseDTO
            {
                Id = team.Id,
                Name = team.Name,
                ContactInfo = team.ContactInfo,
                LocationId = team.LocationId,
                Address = team.Address,
                Status = team.Status,
                TeamLeaderName = team.TeamLeaderName,
                SocialMediaURL = team.SocialMediaUrl,
                Email = team.Email,
                Phone = team.Phone,
                NumberOfMembers = team.NumberOfMembers,
                Specialization = team.Specialization,
                EquipmentDetails = team.EquipmentDetails,
                EstablishedDate = team.EstablishedDate,
                CreatedAt = team.CreatedAt,
                UpdatedAt = team.UpdatedAt
            };

            return Result<ReliefTeamResponseDTO>.Success(responseDto, "Relief team created and invitation sent.");
        }
        catch (Exception ex)
        {
            // Ideally log ex
            return Result<ReliefTeamResponseDTO>.Failure($"Failed to create relief team: {ex.Message}");
        }
    }
    
     public async Task<Result<List<ReliefTeamResponseDTO>>> GetAllAsync()
        {
            try
            {
                var teams = await _reliefTeamRepository.GetAllAsync();
                var dtos = teams.Select(t => new ReliefTeamResponseDTO
                {
                    Id = t.Id,
                    Name = t.Name,
                    ContactInfo = t.ContactInfo,
                    LocationId = t.LocationId,
                    Address = t.Address,
                    Status = t.Status,
                    TeamLeaderName = t.TeamLeaderName,
                    SocialMediaURL = t.SocialMediaUrl,
                    Email = t.Email,
                    Phone = t.Phone,
                    NumberOfMembers = t.NumberOfMembers,
                    Specialization = t.Specialization,
                    EquipmentDetails = t.EquipmentDetails,
                    EstablishedDate = t.EstablishedDate,
                    CreatedAt = t.CreatedAt,
                    UpdatedAt = t.UpdatedAt
                }).ToList();

                return Result<List<ReliefTeamResponseDTO>>.Success(dtos);
            }
            catch (Exception ex)
            {
                return Result<List<ReliefTeamResponseDTO>>.Failure($"Error getting relief teams: {ex.Message}");
            }
        }

        public async Task<Result<ReliefTeamResponseDTO>> GetByIdAsync(int id)
        {
            try
            {
                var team = await _reliefTeamRepository.GetByIdAsync(id);
                if (team == null)
                    return Result<ReliefTeamResponseDTO>.NotFoundError("Relief team not found.");

                var dto = new ReliefTeamResponseDTO
                {
                    Id = team.Id,
                    Name = team.Name,
                    ContactInfo = team.ContactInfo,
                    LocationId = team.LocationId,
                    Address = team.Address,
                    Status = team.Status,
                    TeamLeaderName = team.TeamLeaderName,
                    SocialMediaURL = team.SocialMediaUrl,
                    Email = team.Email,
                    Phone = team.Phone,
                    NumberOfMembers = team.NumberOfMembers,
                    Specialization = team.Specialization,
                    EquipmentDetails = team.EquipmentDetails,
                    EstablishedDate = team.EstablishedDate,
                    CreatedAt = team.CreatedAt,
                    UpdatedAt = team.UpdatedAt
                };

                return Result<ReliefTeamResponseDTO>.Success(dto);
            }
            catch (Exception ex)
            {
                return Result<ReliefTeamResponseDTO>.Failure($"Error retrieving relief team: {ex.Message}");
            }
        }
        
        public async Task<Result<OperationResponseDto>> UpdateAsync(int id, UpdateReliefTeamRequestDto dto)
        {
            try
            {
                var team = await _reliefTeamRepository.GetByIdAsync(id);
                if (team == null)
                    return Result<OperationResponseDto>.NotFoundError("Relief team not found.");

                // Update properties
                team.Name = dto.Name;
                team.Email = dto.Email;
                team.ContactInfo = dto.ContactInfo;
                //team.LocationId = dto.LocationId;
                team.Address = dto.Address;
                team.Status = dto.Status ?? team.Status;
                team.TeamLeaderName = dto.TeamLeaderName;
                team.SocialMediaUrl = dto.SocialMediaURL;
                team.Phone = dto.Phone;
                team.NumberOfMembers = dto.NumberOfMembers;
                team.Specialization = dto.Specialization;
                team.EquipmentDetails = dto.EquipmentDetails;
                team.EstablishedDate = dto.EstablishedDate;
                team.UpdatedAt = DateTime.UtcNow;

                await _reliefTeamRepository.UpdateAsync(team);

                return Result<OperationResponseDto>.Success(new OperationResponseDto { Message = "Relief team updated successfully." });
            }
            catch (Exception ex)
            {
                return Result<OperationResponseDto>.Failure($"Error updating relief team: {ex.Message}");
            }
        }

        public async Task<Result<OperationResponseDto>> DeleteAsync(int id)
        {
            try
            {
                var team = await _reliefTeamRepository.GetByIdAsync(id);
                if (team == null)
                    return Result<OperationResponseDto>.NotFoundError("Relief team not found.");

                await _reliefTeamRepository.DeleteAsync(team);
                return Result<OperationResponseDto>.Success(new OperationResponseDto { Message = "Relief team deleted successfully." });
            }
            catch (Exception ex)
            {
                return Result<OperationResponseDto>.Failure($"Error deleting relief team: {ex.Message}");
            }
        }

}

using DisasterManagementSystem_Data.Models;
using DisasterManagementSystem_Data.Repositories.Interfaces;
using DisasterManagementSystem_Services.Models;
using DisasterManagementSystem_Services.Models.ReliefTeamActivityDtos;
using DisasterManagementSystem_Services.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisasterManagementSystem_Services.Services.Implements
{
    public class ReliefTeamActivityService : IReliefTeamActivityService
    {
        private readonly IReliefTeamActivityRepository _activityRepository;
        private readonly IUserRepository _userRepository;
        private readonly IReliefTeamRepository _reliefTeamRepository; // Ensure this exists
        private readonly IReportPhotoService _photoService;
        private readonly AppDbContext _context;
        private readonly ILogger<ReliefTeamActivityService> _logger;

        public ReliefTeamActivityService(
            IReliefTeamActivityRepository activityRepository,
            IUserRepository userRepository,
            IReliefTeamRepository reliefTeamRepository, // Must be included
            IReportPhotoService photoService,
            AppDbContext context,
            ILogger<ReliefTeamActivityService> logger)
        {
            _activityRepository = activityRepository ?? throw new ArgumentNullException(nameof(activityRepository));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _reliefTeamRepository = reliefTeamRepository ?? throw new ArgumentNullException(nameof(reliefTeamRepository)); // Critical fix
            _photoService = photoService ?? throw new ArgumentNullException(nameof(photoService));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<ReliefTeamActivityDTO>> CreateAsync(CreateReliefTeamActivityDTO dto, Guid currentUserId)
        {
            ReliefTeamActivity createdActivity = null;
            try
            {
                // Validate user exists
                var user = await _userRepository.GetByIdAsync(currentUserId);
                if (user == null)
                    return Result<ReliefTeamActivityDTO>.ValidationError("User not found");

                // Validate relief team exists
                var team = await _reliefTeamRepository.GetByIdAsync(dto.ReliefTeamId);
                if (team == null)
                    return Result<ReliefTeamActivityDTO>.ValidationError("Relief team not found");

                // Create activity entity
                var activity = new ReliefTeamActivity
                {
                    ReliefTeamId = dto.ReliefTeamId,
                    PostedBy = currentUserId,
                    ActivityDate = dto.ActivityDate,
                    Title = dto.Title,
                    Description = dto.Description,
                    DetailedAddress = dto.DetailedAddress,
                    CreatedAt = DateTime.UtcNow,
                    ActivityType = dto.ActivityType,
                    PeopleHelped = dto.PeopleHelped,
                    ItemsDistributed = dto.ItemsDistributed,
                    ExpenseAmount = dto.ExpenseAmount
                };

                // Save activity to get ID
                createdActivity = await _activityRepository.AddAsync(activity);

                // Upload media files if any
                List<ActivityMediaDTO> mediaResults = new List<ActivityMediaDTO>();
                if (dto.MediaFiles != null && dto.MediaFiles.Count > 0)
                {
                    var mediaResult = await _photoService.UploadActivityPhotosAsync(
                        createdActivity.Id,
                        dto.MediaFiles.ToArray()
                    );

                    // PROPER RESULT CHECKING USING YOUR RESULT<T> STRUCTURE
                    if (mediaResult.IsSuccess)
                    {
                        mediaResults = mediaResult.Data?.Select(m => new ActivityMediaDTO
                        {
                            Id = m.Id,
                            FilePath = m.FilePath,
                            FileType = m.FileType,
                            FileSize = m.FileSize,
                            UploadedAt = m.UploadedAt,
                            IsVideo = m.IsVideo
                        }).ToList() ?? new List<ActivityMediaDTO>();
                    }
                    else
                    {
                        // Rollback activity creation if media upload fails
                        await _activityRepository.DeleteAsync(createdActivity);
                        return Result<ReliefTeamActivityDTO>.Failure(
                            mediaResult.Message ?? "Media upload failed");
                    }
                }

                // Map to DTO
                var activityDto = new ReliefTeamActivityDTO
                {
                    Id = createdActivity.Id,
                    ReliefTeamId = createdActivity.ReliefTeamId,
                    PostedBy = createdActivity.PostedBy,
                    ActivityDate = createdActivity.ActivityDate,
                    Title = createdActivity.Title,
                    Description = createdActivity.Description,
                    DetailedAddress = createdActivity.DetailedAddress,
                    CreatedAt = createdActivity.CreatedAt,
                    ActivityType = createdActivity.ActivityType,
                    PeopleHelped = createdActivity.PeopleHelped,
                    ItemsDistributed = createdActivity.ItemsDistributed,
                    ExpenseAmount = createdActivity.ExpenseAmount,
                    ReliefTeamName = activity.ReliefTeam?.Name ?? "Unknown Team",
                    PostedByUserName = activity.PostedByNavigation?.Name ?? "Unknown User",

                    Media = mediaResults
                };

                return Result<ReliefTeamActivityDTO>.Success(activityDto, "Activity created successfully");
            }
            catch (Exception ex)
            {
                // Cleanup if any error occurs after activity creation
                if (createdActivity != null && createdActivity.Id > 0)
                {
                    await _activityRepository.DeleteAsync(createdActivity);
                }
                return Result<ReliefTeamActivityDTO>.Failure($"Error creating activity: {ex.Message}");
            }
        }

        public async Task<Result<bool>> DeleteAsync(int id, Guid currentUserId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var activity = await _activityRepository.GetByIdAsync(id, includeMedia: true, includeRelated: true);
                if (activity == null)
                    return Result<bool>.NotFoundError("Activity not found");

                // Authorization
                var currentUser = await _userRepository.GetByIdAsync(currentUserId);
                if (currentUser?.Role != "Admin" && activity.PostedBy != currentUserId)
                    return Result<bool>.ValidationError("Unauthorized");

                // Collect file paths BEFORE deletion
                var filePaths = activity.ReportPhotos?
                    .Select(p => p.FilePath)
                    .ToList() ?? new List<string>();

                // Delete activity (will cascade to photos via EF)
                _context.ReliefTeamActivities.Remove(activity);
                var saveResult = await _context.SaveChangesAsync() > 0;

                if (!saveResult)
                    return Result<bool>.Failure("Activity deletion failed");

                await transaction.CommitAsync();

                // Delete from Cloudinary AFTER successful DB commit
                foreach (var path in filePaths)
                {
                    try
                    {
                        await _photoService.DeleteCloudinaryFile(path);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Failed to delete Cloudinary file: {path}");
                    }
                }

                return Result<bool>.Success(true, "Activity deleted");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Result<bool>.Failure($"Deletion error: {ex.Message}");
            }
        }

        public async Task<Result<List<ReliefTeamActivityDTO>>> GetAllAsync()
        {
            try
            {
                var activities = await _activityRepository.GetAllAsync(includeMedia: true,includeRelated:true);
                var dtos = activities.Select(MapToDTO).ToList();
                return Result<List<ReliefTeamActivityDTO>>.Success(dtos);
            }
            catch (Exception ex)
            {
                return Result<List<ReliefTeamActivityDTO>>.Failure($"Error retrieving activities: {ex.Message}");
            }
        }
        public async Task<Result<List<ReliefTeamActivityDTO>>> GetActivitiesByUserAsync(Guid userId)
        {
            try
            {
                var activities = await _activityRepository.GetByUserIdAsync(userId, includeMedia: true);
                var dtos = activities.Select(MapToDTO).ToList();
                return Result<List<ReliefTeamActivityDTO>>.Success(dtos);
            }
            catch (Exception ex)
            {
                return Result<List<ReliefTeamActivityDTO>>.Failure($"Error retrieving user activities: {ex.Message}");
            }
        }

        public async Task<Result<ReliefTeamActivityDTO>> GetByIdAsync(int id)
        {
            try
            {
                var activity = await _activityRepository.GetByIdAsync(id, includeMedia: true,includeRelated:true);
                if (activity == null)
                    return Result<ReliefTeamActivityDTO>.NotFoundError("Activity not found");

                return Result<ReliefTeamActivityDTO>.Success(MapToDTO(activity));
            }
            catch (Exception ex)
            {
                return Result<ReliefTeamActivityDTO>.Failure($"Error retrieving activity: {ex.Message}");
            }
        }


        public async Task<Result<ReliefTeamActivityDTO>> UpdateAsync(UpdateReliefTeamActivityDTO dto, Guid currentUserId)
        {
            try
            {
                var activity = await _activityRepository.GetByIdAsync(dto.Id, includeMedia: true);
                if (activity == null)
                    return Result<ReliefTeamActivityDTO>.NotFoundError("Activity not found");

                // Authorization
                var currentUser = await _userRepository.GetByIdAsync(currentUserId);
                if (currentUser?.Role != "Admin" && activity.PostedBy != currentUserId)
                    return Result<ReliefTeamActivityDTO>.ValidationError("Unauthorized");

                // Update properties
                activity.ReliefTeamId = dto.ReliefTeamId;
                activity.ActivityDate = dto.ActivityDate;
                activity.Title = dto.Title;
                activity.Description = dto.Description;
                activity.DetailedAddress = dto.DetailedAddress;
                activity.ActivityType = dto.ActivityType;
                activity.PeopleHelped = dto.PeopleHelped;
                activity.ItemsDistributed = dto.ItemsDistributed;
                activity.ExpenseAmount = dto.ExpenseAmount;

                // Handle media deletions
                if (dto.MediaIdsToDelete != null && dto.MediaIdsToDelete.Any())
                {
                    foreach (var mediaId in dto.MediaIdsToDelete)
                    {
                        await _photoService.DeletePhotoAsync(mediaId);
                    }
                }

                // Handle new media uploads
                List<ActivityMediaDTO> newMedia = new();
                if (dto.NewMediaFiles != null && dto.NewMediaFiles.Any())
                {
                    var uploadResult = await _photoService.UploadActivityPhotosAsync(
                        activity.Id,
                        dto.NewMediaFiles.ToArray()
                    );

                    if (uploadResult.IsSuccess)
                    {
                        newMedia = uploadResult.Data.Select(m => new ActivityMediaDTO
                        {
                            Id = m.Id,
                            FilePath = m.FilePath,
                            FileType = m.FileType,
                            FileSize = m.FileSize,
                            UploadedAt = m.UploadedAt,
                            IsVideo = m.IsVideo
                        }).ToList();
                    }
                    else
                    {
                        return Result<ReliefTeamActivityDTO>.Failure(uploadResult.Message);
                    }
                }

                // Update activity in database
                var updatedActivity = await _activityRepository.UpdateAsync(activity);

                // Refetch to get updated media
                var refreshedActivity = await _activityRepository.GetByIdAsync(updatedActivity.Id, includeMedia: true);
                return Result<ReliefTeamActivityDTO>.Success(MapToDTO(refreshedActivity));
            }
            catch (Exception ex)
            {
                return Result<ReliefTeamActivityDTO>.Failure($"Update failed: {ex.Message}");
            }
        }

        public async Task<Result<List<ReliefTeamActivityDTO>>> GetActivitiesByTeamAsync(int teamId)
        {
            try
            {
                var activities = await _activityRepository.GetByTeamIdAsync(teamId, includeMedia: true);
                var dtos = activities.Select(MapToDTO).ToList();
                return Result<List<ReliefTeamActivityDTO>>.Success(dtos);
            }
            catch (Exception ex)
            {
                return Result<List<ReliefTeamActivityDTO>>.Failure($"Error retrieving team activities: {ex.Message}");
            }
        }

        public async Task<Result<List<ReliefTeamActivityDTO>>> GetActivitiesByTypeAsync(string activityType)
        {
            try
            {
                var activities = await _activityRepository.GetByTypeAsync(activityType, includeMedia: true);
                var dtos = activities.Select(MapToDTO).ToList();
                return Result<List<ReliefTeamActivityDTO>>.Success(dtos);
            }
            catch (Exception ex)
            {
                return Result<List<ReliefTeamActivityDTO>>.Failure($"Error retrieving activities by type: {ex.Message}");
            }
        }

        public async Task<Result<ActivityStatsDTO>> GetActivityStatsAsync()
        {
            try
            {
                var stats = new ActivityStatsDTO
                {
                    TotalActivities = await _activityRepository.GetCountAsync(),
                    ActivitiesByType = await _activityRepository.GetCountByTypeAsync(),
                    RecentActivities = (await _activityRepository.GetRecentAsync(5))
                        .Select(MapToDTO).ToList()
                };
                return Result<ActivityStatsDTO>.Success(stats);
            }
            catch (Exception ex)
            {
                return Result<ActivityStatsDTO>.Failure($"Error retrieving stats: {ex.Message}");
            }
        }


        private ReliefTeamActivityDTO MapToDTO(ReliefTeamActivity activity)
        {
            return new ReliefTeamActivityDTO
            {
                Id = activity.Id,
                ReliefTeamId = activity.ReliefTeamId,
                PostedBy = activity.PostedBy,
                ActivityDate = activity.ActivityDate,
                Title = activity.Title,
                Description = activity.Description,
                DetailedAddress = activity.DetailedAddress,
                CreatedAt = activity.CreatedAt,
                ActivityType = activity.ActivityType,
                PeopleHelped = activity.PeopleHelped,
                ItemsDistributed = activity.ItemsDistributed,
                ExpenseAmount = activity.ExpenseAmount,
                ReliefTeamName = activity.ReliefTeam?.Name ?? "Unknown Team",
                PostedByUserName = activity.PostedByNavigation?.Name ?? "Unknown User",
                Media = activity.ReportPhotos?.Select(p => new ActivityMediaDTO
                {
                    Id = p.Id,
                    FilePath = p.FilePath,
                    FileType = p.FileType,
                    FileSize = p.FileSize,
                    UploadedAt = p.UploadedAt,
                    IsVideo = p.FileType == "video"
                }).ToList() ?? new List<ActivityMediaDTO>()
            };
        }
    }
}

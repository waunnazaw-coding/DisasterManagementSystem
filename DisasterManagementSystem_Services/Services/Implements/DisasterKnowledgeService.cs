using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using DisasterManagementSystem_Data.Models;
using DisasterManagementSystem_Data.Repositories.Interfaces;
using DisasterManagementSystem_Services.Models;
using DisasterManagementSystem_Services.Models.DisasterKnowledgeDtos;
using DisasterManagementSystem_Services.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Resource = DisasterManagementSystem_Data.Models.Resource;

namespace DisasterManagementSystem_Services.Services.Implements;

public class DisasterKnowledgeService : IDisasterKnowledgeService
{
    private readonly IDisasterKnowledgeRepository _disasterRepo;
    private readonly IResourceRepository _resourceRepo;
    private readonly Cloudinary _cloudinary;
    private readonly AppDbContext _dbContext; // EF DbContext for transaction management

    public DisasterKnowledgeService(
        IDisasterKnowledgeRepository disasterRepo,
        IResourceRepository resourceRepo,
        IOptions<CloudinarySettings> config,
        AppDbContext dbContext)
    {
        _disasterRepo = disasterRepo;
        _resourceRepo = resourceRepo;
        _dbContext = dbContext;

        var account = new Account(
            config.Value.CloudName,
            config.Value.ApiKey,
            config.Value.ApiSecret);
        _cloudinary = new Cloudinary(account);
    }

    // Create DisasterKnowledge + multiple resources atomically
    public async Task<DisasterKnowledgeResponseDto> CreateAsync(
        DisasterKnowledgeRequestDto dto,
        List<ResourceRequestDto> resourcesDto)
    {
        using var transaction = await _dbContext.Database.BeginTransactionAsync();

        try
        {
            var disasterKnowledge = new DisasterKnowledge
            {
                Title = dto.Title,
                ContentType = dto.ContentType,
                DisasterType = dto.DisasterType,
                AuthorId = dto.AuthorId,
                Content = dto.Content,
                Language = dto.Language,
                ReferenceFrom = dto.ReferenceFrom,
                DateCreated = DateTime.UtcNow,
                DateUpdated = DateTime.UtcNow
            };

            disasterKnowledge = await _disasterRepo.AddAsync(disasterKnowledge);

            var createdResources = new List<Resource>();

            if (resourcesDto != null && resourcesDto.Any())
            {
                foreach (var resourceDto in resourcesDto)
                {
                    if (resourceDto.File == null || resourceDto.File.Length == 0)
                        throw new Exception("Resource file is required for all resources.");

                    var uploadResult = await UploadToCloudinary(resourceDto.File);

                    var resource = new Resource
                    {
                        DisasterKnowledgeId = disasterKnowledge.Id,
                        ResourceType = resourceDto.ResourceType,
                        Url = uploadResult.SecureUrl.AbsoluteUri,
                        Description = resourceDto.Description,
                        DateAdded = DateTime.UtcNow
                    };
                    createdResources.Add(await _resourceRepo.AddAsync(resource));
                }
            }

            await transaction.CommitAsync();

            disasterKnowledge.Resources = createdResources;

            return MapToResponseDto(disasterKnowledge);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    // Get all DisasterKnowledge including related resources
    public async Task<IEnumerable<DisasterKnowledgeResponseDto>> GetAllAsync()
    {
        // Get all DisasterKnowledge entries including their Resources
        var allDisasterKnowledge = await _disasterRepo.GetAllAsync();

        return allDisasterKnowledge.Select(MapToResponseDto);
    }

    // Get DisasterKnowledge by Id including resources
    public async Task<DisasterKnowledgeResponseDto?> GetByIdAsync(int id)
    {
        var entity = await _disasterRepo.GetByIdAsync(id);
        return entity == null ? null : MapToResponseDto(entity);
    }

    // Update DisasterKnowledge + update / add / remove resources in a transaction
    public async Task<DisasterKnowledgeResponseDto?> UpdateAsync(
        int id,
        DisasterKnowledgeRequestDto dto,
        List<(int? ResourceId, ResourceRequestDto ResourceDto)>? resourceUpdates = null)
    {
        using var transaction = await _dbContext.Database.BeginTransactionAsync();

        try
        {
            var original = await _disasterRepo.GetByIdAsync(id);
            if (original == null)
                return null;

            original.Title = dto.Title;
            original.ContentType = dto.ContentType;
            original.DisasterType = dto.DisasterType;
            original.AuthorId = dto.AuthorId;
            original.Content = dto.Content;
            original.Language = dto.Language;
            original.ReferenceFrom = dto.ReferenceFrom;
            original.DateUpdated = DateTime.UtcNow;

            var updated = await _disasterRepo.UpdateAsync(original);
            if (updated == null)
            {
                await transaction.RollbackAsync();
                return null;
            }

            var existingResources = await _resourceRepo.GetByDisasterKnowledgeIdAsync(id);
            var existingResourceIds = existingResources.Select(r => r.Id).ToHashSet();
            var resourceIdsToKeep = new HashSet<int>();

            if (resourceUpdates != null)
            {
                foreach (var (resourceId, resourceDto) in resourceUpdates)
                {
                    if (resourceId == null)
                    {
                        if (resourceDto.File == null || resourceDto.File.Length == 0)
                            throw new Exception("File is required for new resources.");

                        var uploadResult = await UploadToCloudinary(resourceDto.File);
                        var newResource = new Resource
                        {
                            DisasterKnowledgeId = id,
                            ResourceType = resourceDto.ResourceType,
                            Url = uploadResult.SecureUrl.AbsoluteUri,
                            Description = resourceDto.Description,
                            DateAdded = DateTime.UtcNow
                        };
                        await _resourceRepo.AddAsync(newResource);
                    }
                    else
                    {
                        var existingResource = existingResources.FirstOrDefault(r => r.Id == resourceId);
                        if (existingResource == null)
                            throw new Exception($"Resource with ID {resourceId} not found.");

                        resourceIdsToKeep.Add(existingResource.Id);

                        var isFileUpdated = resourceDto.File != null && resourceDto.File.Length > 0;
                        if (isFileUpdated)
                        {
                            var uploadResult = await UploadToCloudinary(resourceDto.File);
                            await DeleteFromCloudinary(existingResource.Url);
                            existingResource.Url = uploadResult.SecureUrl.AbsoluteUri;
                        }

                        existingResource.ResourceType = resourceDto.ResourceType;
                        existingResource.Description = resourceDto.Description;

                        await _resourceRepo.UpdateAsync(existingResource);
                    }
                }

                var toDelete = existingResourceIds.Except(resourceIdsToKeep).ToList();

                foreach (var deleteId in toDelete)
                {
                    var resourceToDelete = existingResources.First(r => r.Id == deleteId);
                    await DeleteFromCloudinary(resourceToDelete.Url);
                    await _resourceRepo.DeleteAsync(deleteId);
                }
            }

            await transaction.CommitAsync();

            var updatedEntity = await _disasterRepo.GetByIdAsync(id);
            return updatedEntity == null ? null : MapToResponseDto(updatedEntity);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    // Delete DisasterKnowledge and all related resources and their files
    public async Task<bool> DeleteAsync(int id)
    {
        using var transaction = await _dbContext.Database.BeginTransactionAsync();

        try
        {
            var resources = await _resourceRepo.GetByDisasterKnowledgeIdAsync(id);
            foreach (var resource in resources)
            {
                await DeleteFromCloudinary(resource.Url);
                await _resourceRepo.DeleteAsync(resource.Id);
            }

            var success = await _disasterRepo.DeleteAsync(id);
            if (!success)
            {
                await transaction.RollbackAsync();
                return false;
            }

            await transaction.CommitAsync();
            return true;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    // Helper: Upload a file to Cloudinary and return the upload result
    private async Task<ImageUploadResult> UploadToCloudinary(IFormFile file)
    {
        await using var stream = file.OpenReadStream();
        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(file.FileName, stream),
            UseFilename = true,
            UniqueFilename = true,
            Overwrite = false,
            Folder = "disaster_resources",
            Transformation = new Transformation().Quality("auto").FetchFormat("auto")
        };
        var result = await _cloudinary.UploadAsync(uploadParams);

        if (result.StatusCode != System.Net.HttpStatusCode.OK || result.SecureUrl == null)
            throw new Exception($"Cloudinary upload failed: {result.Error?.Message ?? "Unknown error"}");

        return result;
    }

    // Helper: Delete file from Cloudinary based on URL
    private async Task DeleteFromCloudinary(string fileUrl)
    {
        if (string.IsNullOrEmpty(fileUrl)) return;

        var publicId = GetPublicIdFromUrl(fileUrl);
        if (!string.IsNullOrEmpty(publicId))
        {
            var deleteParams = new DeletionParams(publicId);
            await _cloudinary.DestroyAsync(deleteParams);
        }
    }

    // Helper: Extract Cloudinary public ID from URL
    private string? GetPublicIdFromUrl(string url)
    {
        try
        {
            var uri = new Uri(url);
            var segments = uri.Segments;
            if (segments.Length < 3) return null;

            var publicIdWithExtension = segments[^1];
            return publicIdWithExtension.Split('.')[0];
        }
        catch
        {
            return null;
        }
    }

    // Map domain model to response DTO (including resources)
    private DisasterKnowledgeResponseDto MapToResponseDto(DisasterKnowledge entity)
    {
        return new DisasterKnowledgeResponseDto
        {
            Id = entity.Id,
            Title = entity.Title,
            ContentType = entity.ContentType,
            DisasterType = entity.DisasterType,
            AuthorId = entity.AuthorId,
            Content = entity.Content,
            Language = entity.Language,
            ReferenceFrom = entity.ReferenceFrom,
            DateCreated = entity.DateCreated,
            DateUpdated = entity.DateUpdated,
            Resources = entity.Resources?.Select(r => new ResourceResponseDto
            {
                Id = r.Id,
                ResourceType = r.ResourceType,
                Url = r.Url,
                Description = r.Description,
                DateAdded = r.DateAdded
            }).ToList()
        };
    }
}
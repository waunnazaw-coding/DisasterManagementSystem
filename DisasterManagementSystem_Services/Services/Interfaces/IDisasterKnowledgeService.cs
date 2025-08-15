using DisasterManagementSystem_Services.Models.DisasterKnowledgeDtos;

namespace DisasterManagementSystem_Services.Services.Interfaces;


public interface IDisasterKnowledgeService
{
    Task<IEnumerable<DisasterKnowledgeResponseDto>> GetAllAsync();
    Task<DisasterKnowledgeResponseDto?> GetByIdAsync(int id);
    
    // Create new DisasterKnowledge entry with multiple resources
    Task<DisasterKnowledgeResponseDto> CreateAsync(
        DisasterKnowledgeRequestDto dto,
        List<ResourceRequestDto> resourcesDto);
    
    // Update DisasterKnowledge and manage its resources (add/update/delete)
    Task<DisasterKnowledgeResponseDto?> UpdateAsync(
        int id,
        DisasterKnowledgeRequestDto dto,
        List<(int? ResourceId, ResourceRequestDto ResourceDto)>? resourceUpdates = null);
    
    // Delete DisasterKnowledge and all related resources/media
    Task<bool> DeleteAsync(int id);
}
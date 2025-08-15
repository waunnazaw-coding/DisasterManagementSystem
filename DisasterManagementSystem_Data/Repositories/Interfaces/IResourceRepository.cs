using DisasterManagementSystem_Data.Models;

namespace DisasterManagementSystem_Data.Repositories.Interfaces;

public interface IResourceRepository
{
    Task<Resource?> GetByIdAsync(int id);
    Task<IEnumerable<Resource>> GetByDisasterKnowledgeIdAsync(int disasterKnowledgeId);
    Task<Resource> AddAsync(Resource resource);
    Task<Resource?> UpdateAsync(Resource entity);
    Task<bool> DeleteAsync(int id);
}
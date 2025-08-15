using DisasterManagementSystem_Data.Models;

namespace DisasterManagementSystem_Data.Repositories.Interfaces;

public interface IDisasterKnowledgeRepository
{
    Task<DisasterKnowledge?> GetByIdAsync(int id);
    Task<IEnumerable<DisasterKnowledge>> GetAllAsync();
    Task<DisasterKnowledge> AddAsync(DisasterKnowledge disasterKnowledge);
    Task<DisasterKnowledge?> UpdateAsync(DisasterKnowledge disasterKnowledge);
    Task<bool> DeleteAsync(int id);
}

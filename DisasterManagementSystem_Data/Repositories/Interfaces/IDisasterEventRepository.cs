using DisasterManagementSystem_Data.Models;

namespace DisasterManagementSystem_Data.Repositories
{
    public interface IDisasterEventRepository
    {
        Task<IEnumerable<DisasterEvent>> GetAllAsync();
        Task<IEnumerable<DisasterEvent>> GetAllActive();
        Task<int> CountVerifiedAsync();
        Task<DisasterEvent?> GetByIdAsync(int id);
        Task<IEnumerable<DisasterEvent>> SearchByNameAsync(string name);
        Task AddAsync(DisasterEvent disasterEvent);
        Task UpdateAsync(DisasterEvent disasterEvent);
        Task DeleteAsync(int id);
    }
}
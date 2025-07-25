using DisasterManagementSystem_Data.Models;
using DisasterManagementSystem_Services.Models;

namespace DisasterManagementSystem_Data.Service
{
    public interface IDisasterEventService
    {
        Task<Result<DisasterEvent>> GetByIdAsync(int id);
        Task<Result<IEnumerable<DisasterEvent>>> GetAllAsync();
        Task<Result<EventFormCreateDto>> AddEventFormAsync(EventFormCreateDto dto);
        Task<Result<DisasterEvent>> UpdateAsync(DisasterEvent disasterEvent);
        Task<Result<bool>> DeleteAsync(int id);
    }
}
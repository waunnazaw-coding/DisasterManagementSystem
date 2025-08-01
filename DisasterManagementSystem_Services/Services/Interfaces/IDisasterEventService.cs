using DisasterManagementSystem_Data.Models;
using DisasterManagementSystem_Services.Models;

namespace DisasterManagementSystem_Data.Service
{
    public interface IDisasterEventService
    {
        Task<Result<DisasterEventListDto>> GetByIdAsync(int id);
        Task<Result<EventFormUpdateDto>> GetByIdForUpdateAsync(int eventId);
        Task<Result<DisasterEventListDto>> GetByIdWithLocationAsync(int eventId);
        Task<Result<IEnumerable<DisasterEventListDto>>> GetAllAsync();
        Task<List<DisasterEventListDto>> GetAllWithAffectedPeopleAsync();
        Task<IEnumerable<DisasterEventListDto>> SearchByNameAsync(string name);
        Task<Result<EventFormCreateDto>> AddEventFormAsync(EventFormCreateDto dto);
        Task<Result<EventFormUpdateDto>> UpdateEventFormAsync(EventFormUpdateDto dto);
        Task<Result<bool>> DeleteAsync(int id);
    }
}
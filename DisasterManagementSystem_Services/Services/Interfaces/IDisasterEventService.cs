using DisasterManagementSystem_Data.Models;
using DisasterManagementSystem_Services.Models;

namespace DisasterManagementSystem_Services.Service
{
    public interface IDisasterEventService
    {
        Task<Result<DisasterEventListDto>> GetByIdAsync(int id);
        Task<Result<EventFormUpdateDto>> GetByIdForUpdateAsync(int eventId);
        Task<Result<DisasterEventDetailsDto>> GetByIdWithLocationAsync(int eventId);
        Task<Result<IEnumerable<DisasterEventListDto>>> GetAllAsync();
        Task<Result<IEnumerable<DisasterEventListDto>>> GetAllActiveAsync();
        Task<Result<IEnumerable<DisasterEventListDto>>> GetAllForMapViewAsync();
        Task<Result<int>> GetActiveCountAsync();
        Task<List<DisasterEventListDto>> GetAllWithAffectedPeopleAsync();
        Task<IEnumerable<DisasterEventListDto>> SearchByNameAsync(string name);
        Task<Result<EventFormCreateDto>> AddEventFormAsync(EventFormCreateDto dto);
        Task<Result<EventFormCreateDto>> ReportToEventFormAsync(EventFormCreateDto dto);
        Task<Result<EventFormUpdateDto>> UpdateEventFormAsync(EventFormUpdateDto dto);
        Task<Result<bool>> DeleteAsync(int id);
    }
}
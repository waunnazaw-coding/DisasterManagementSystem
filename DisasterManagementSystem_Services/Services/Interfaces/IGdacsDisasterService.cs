using DisasterManagementSystem_Data.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DisasterManagementSystem_Services.Services.Interfaces
{
    public interface IGdacsDisasterService
    {
        Task<IEnumerable<GdacsdisasterEvent>> FetchFromFeedAsync();
        Task<IEnumerable<GdacsdisasterEvent>> GetAllEventsAsync();
        Task<List<GdacsdisasterEvent>> GetEventsForCurrentWeekAsync();
        Task<List<GdacsdisasterEvent>> GetTodaysEventsAsync();
        Task UpsertAsync(GdacsdisasterEvent disasterEvent);
    }
}

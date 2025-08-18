using DisasterManagementSystem_Data.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DisasterManagementSystem_Services.Services.Interfaces
{
    public interface IGdacsDisasterService
    {
        Task<IEnumerable<GdacsdisasterEvent>> FetchFromFeedAsync();
        Task UpsertAsync(GdacsdisasterEvent disasterEvent);
    }
}

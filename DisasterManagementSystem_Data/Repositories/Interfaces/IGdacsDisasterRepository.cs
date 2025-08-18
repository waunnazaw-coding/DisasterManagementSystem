using DisasterManagementSystem_Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisasterManagementSystem_Data.Repositories.Interfaces
{
    public interface IGdacsDisasterRepository
    {
        Task<GdacsdisasterEvent?> GetByEventIdAsync(string eventId);
        Task UpsertAsync(GdacsdisasterEvent disasterEvent);
        Task<IEnumerable<GdacsdisasterEvent>> GetAllAsync();
    }
}

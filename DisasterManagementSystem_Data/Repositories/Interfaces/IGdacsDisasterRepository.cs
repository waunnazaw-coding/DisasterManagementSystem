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
        Task<List<GdacsdisasterEvent>> GetEventsByDateAsync(DateTime date);
        Task UpsertAsync(GdacsdisasterEvent disasterEvent);
        Task<IEnumerable<GdacsdisasterEvent>> GetAllAsync();
        Task<List<GdacsdisasterEvent>> GetEventsByDateRangeAsync(DateTime startDate, DateTime endDate);
    }
}

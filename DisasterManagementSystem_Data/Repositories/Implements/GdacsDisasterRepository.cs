using DisasterManagementSystem_Data.Models;
using DisasterManagementSystem_Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisasterManagementSystem_Data.Repositories.Implements
{
    public class GdacsDisasterRepository : IGdacsDisasterRepository
    {
        private readonly AppDbContext _context;

        public GdacsDisasterRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<GdacsdisasterEvent?> GetByEventIdAsync(string eventId)
        {
            return await _context.GdacsdisasterEvents.FindAsync(eventId);
        }

        public async Task UpsertAsync(GdacsdisasterEvent disasterEvent)
        {
            var existing = await GetByEventIdAsync(disasterEvent.EventId);
            if (existing == null)
            {
                _context.GdacsdisasterEvents.Add(disasterEvent);
            }
            else
            {
                // Update fields
                existing.EventType = disasterEvent.EventType;
                existing.Severity = disasterEvent.Severity;
                existing.EventDate = disasterEvent.EventDate;
                existing.Latitude = disasterEvent.Latitude;
                existing.Longitude = disasterEvent.Longitude;
                existing.LocationAddress = disasterEvent.LocationAddress;
                existing.Impact = disasterEvent.Impact;
                existing.Status = disasterEvent.Status;
            }

            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<GdacsdisasterEvent>> GetAllAsync()
        {
            return await _context.Set<GdacsdisasterEvent>().ToListAsync();
        }

        public async Task<List<GdacsdisasterEvent>> GetEventsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Set<GdacsdisasterEvent>()
                .Where(e => e.EventDate.HasValue && e.EventDate.Value.Date >= startDate.Date && e.EventDate.Value.Date <= endDate.Date)
                .ToListAsync();
        }

        public async Task<List<GdacsdisasterEvent>> GetEventsByDateAsync(DateTime date)
        {
            return await _context.Set<GdacsdisasterEvent>()
                .Where(e => e.EventDate.HasValue && e.EventDate.Value.Date == date.Date)
                .ToListAsync();
        }
    }
}

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
    public class DisasterEventRepository : IDisasterEventRepository
    {

        private readonly AppDbContext _context;

        public DisasterEventRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.DisasterReports.AnyAsync(r => r.Id == id);
        }
        public async Task<DisasterEvent?> GetByIdAsync(int id) =>
        await _context.DisasterEvents.FindAsync(id);

        public async Task<IEnumerable<DisasterEvent>> GetAllAsync()
        {
         return   await _context.DisasterEvents.ToListAsync();
        }
    }
}

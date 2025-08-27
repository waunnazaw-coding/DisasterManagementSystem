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
    public class PartnerRepository : IPartnerRepository
    {
        private readonly AppDbContext _context;

        public PartnerRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Partner> AddAsync(Partner partner)
        {
            _context.Partners.Add(partner);
            await _context.SaveChangesAsync();
            return partner;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var partner = await _context.Partners.FindAsync(id);
            if (partner == null) return false;

            _context.Partners.Remove(partner);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Partners.AnyAsync(e => e.Id == id);
        }

        public async Task<List<Partner>> GetAllAsync()
        {
            return await _context.Partners.ToListAsync();
        }

        public async Task<Partner> GetByIdAsync(int id)
        {
            return await _context.Partners.FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Partner> UpdateAsync(Partner partner)
        {
            _context.Entry(partner).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return partner;
        }
        public async Task<IEnumerable<Partner>> GetPublicPartnersAsync()
        {
            return await _context.Partners
                .Where(p => p.IsPublic && p.Status == "Active")
                .OrderBy(p => p.Name)
                .ToListAsync();
        }
    }
}


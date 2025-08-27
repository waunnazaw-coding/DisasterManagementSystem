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
    public class DonationRepository : IDonationRepository
    {
        private readonly AppDbContext _context;
        public DonationRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task<Donation> CreateAsync(Donation donation)
        {
            await _context.Donations.AddAsync(donation);
            await _context.SaveChangesAsync();
            return donation;
        }

        public async Task<Donation?> GetByIdAsync(int id)
        {
            return await _context.Donations
                .Include(d => d.DonorUser)
                .FirstOrDefaultAsync(d => d.Id == id);
        }

        public async Task<List<Donation>> GetAllAsync()
        {
            return await _context.Donations
                .Include(d => d.DonorUser)
               // .OrderByDescending(d => d.DateReceived) // or whatever your date property is called
                .ToListAsync();
        }

        public async Task<List<Donation>> GetByUserIdAsync(Guid userId)
        {
            return await _context.Donations
                .Include(d => d.DonorUser)
                .Where(d => d.DonorUserId == userId)
                .ToListAsync();
        }

        public async Task UpdateAsync(Donation donation)
        {
            _context.Donations.Update(donation);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var donation = await GetByIdAsync(id);
            if (donation == null) return false;

            _context.Donations.Remove(donation);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<Donation>> GetRecentAsync()
        {
            return await _context.Donations
                .Include(d => d.DonorUser)
                .Where(d => d.Status == "Verified")
                .OrderByDescending(d => d.DateReceived)
                .Take(3) // Get last 5 donations
                .ToListAsync();
        }

        public async Task<int> GetTotalPeopleByPhoneAsync()
        {
            return await _context.Donations
                .Where(d => d.Phone != null)
                .Select(d => d.Phone)
                .Distinct()
                .CountAsync();
        }

        public async Task<decimal?> GetTotalAmountNowYearAsync()
        {
            var currentYear = DateTime.Now.Year;

            return await _context.Donations
                .Where(d => d.DateReceived.HasValue && d.DateReceived.Value.Year == currentYear)
                .SumAsync(d => (decimal?)d.Amount);
        }


        public async Task<decimal?> GetTotalAmount()
        {
            return await _context.Donations
                .Where(d => d.DateReceived != null)
                .SumAsync(d => (decimal?)d.Amount);
        }


        public async Task<decimal?> GetTotalAmountLastYearAsync()
        {
            var lastYear = DateTime.Now.Year - 1;

            return await _context.Donations
                .Where(d => d.DateReceived.HasValue && d.DateReceived.Value.Year == lastYear)
                .SumAsync(d => (decimal?)d.Amount);
        }


    }
}

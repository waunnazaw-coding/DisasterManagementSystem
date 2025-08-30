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
         .Where(d => d.DateReceived.Year == currentYear)
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
        .Where(d => d.DateReceived.Year == lastYear)
        .SumAsync(d => (decimal?)d.Amount);
        }
        public async Task<Dictionary<string, decimal>> GetMonthlyDonationsAsync(int year)
        {
            var monthlyData = await _context.Donations
                .Where(d => d.DateReceived.Year == year && d.Amount != null)
                .GroupBy(d => new { d.DateReceived.Year, d.DateReceived.Month })
                .Select(g => new
                {
                    Month = g.Key.Month,
                    Year = g.Key.Year,
                    TotalAmount = g.Sum(d => d.Amount.Value)
                })
                .ToListAsync();

            var result = new Dictionary<string, decimal>();

            // Initialize all months with 0
            for (int i = 1; i <= 12; i++)
            {
                var monthName = new DateTime(year, i, 1).ToString("MMM");
                result[monthName] = 0;
            }

            // Fill with actual data
            foreach (var item in monthlyData)
            {
                var monthName = new DateTime(item.Year, item.Month, 1).ToString("MMM");
                result[monthName] = item.TotalAmount;
            }

            return result;
        }

        public async Task<Dictionary<int, decimal>> GetYearlyDonationsAsync(int startYear, int endYear)
        {
            var yearlyData = await _context.Donations
                .Where(d => d.DateReceived.Year >= startYear && d.DateReceived.Year <= endYear && d.Amount != null)
                .GroupBy(d => d.DateReceived.Year)
                .Select(g => new
                {
                    Year = g.Key,
                    TotalAmount = g.Sum(d => d.Amount.Value)
                })
                .ToListAsync();

            var result = new Dictionary<int, decimal>();

            // Initialize all years with 0
            for (int year = startYear; year <= endYear; year++)
            {
                result[year] = 0;
            }

            // Fill with actual data
            foreach (var item in yearlyData)
            {
                result[item.Year] = item.TotalAmount;
            }

            return result;
        }

        public async Task<Dictionary<string, decimal>> GetDonationsByCategoryAsync()
        {
            var categoryData = await _context.Donations
                .Where(d => d.Amount != null && d.Category != null)
                .GroupBy(d => d.Category)
                .Select(g => new
                {
                    Category = g.Key,
                    TotalAmount = g.Sum(d => d.Amount.Value)
                })
                .ToListAsync();

            var result = new Dictionary<string, decimal>();

            foreach (var item in categoryData)
            {
                if (item.Category != null)
                {
                    result[item.Category] = item.TotalAmount;
                }
            }

            return result;
        }

    }
}

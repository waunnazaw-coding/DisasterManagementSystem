using DisasterManagementSystem_Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisasterManagementSystem_Data.Repositories.Interfaces
{
    public interface IDonationRepository
    {
        Task<Donation> CreateAsync(Donation donation);
        Task<Donation?> GetByIdAsync(int id);
        Task<List<Donation>> GetAllAsync();
        Task<List<Donation>> GetByUserIdAsync(Guid userId);
        Task UpdateAsync(Donation donation);
        Task<bool> DeleteAsync(int id);
        Task<List<Donation>> GetRecentAsync();
        Task<int> GetTotalPeopleByPhoneAsync();
        Task<decimal?> GetTotalAmountLastYearAsync();
        Task<decimal?> GetTotalAmountNowYearAsync();
        Task<decimal?> GetTotalAmount();
        Task<Dictionary<string, decimal>> GetMonthlyDonationsAsync(int year);
        Task<Dictionary<int, decimal>> GetYearlyDonationsAsync(int startYear, int endYear);
         Task<Dictionary<string, decimal>> GetDonationsByCategoryAsync();
    }
}

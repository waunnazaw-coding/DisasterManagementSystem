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
    }
}

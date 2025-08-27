using DisasterManagementSystem_Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisasterManagementSystem_Data.Repositories.Interfaces
{
    public interface IPartnerRepository
    {
        Task<Partner> GetByIdAsync(int id);
        Task<List<Partner>> GetAllAsync();
        Task<Partner> AddAsync(Partner partner);
        Task<Partner> UpdateAsync(Partner partner);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<IEnumerable<Partner>> GetPublicPartnersAsync();
    }
}

using DisasterManagementSystem_Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisasterManagementSystem_Data.Repositories.Interfaces
{
    public interface IContactRepository
    {
        Task<Contact> GetByIdAsync(int id);
        Task<List<Contact>> GetAllAsync();
        Task AddAsync(Contact contact);
        Task UpdateAsync(Contact contact);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
    }
}

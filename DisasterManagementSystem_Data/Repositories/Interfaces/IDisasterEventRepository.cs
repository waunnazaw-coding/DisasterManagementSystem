using DisasterManagementSystem_Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisasterManagementSystem_Data.Repositories.Interfaces
{
    public interface IDisasterEventRepository
    {
        Task<bool> ExistsAsync(int id);
        Task<IEnumerable<DisasterEvent>> GetAllAsync();
        Task<DisasterEvent?> GetByIdAsync(int id);

    }

}

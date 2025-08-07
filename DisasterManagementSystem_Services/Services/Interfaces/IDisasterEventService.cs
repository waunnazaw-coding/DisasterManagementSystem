using DisasterManagementSystem_Data.Models;
using DisasterManagementSystem_Services.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisasterManagementSystem_Services.Services.Interfaces
{
    public interface IDisasterEventService
    {
        Task<Result<IEnumerable<DisasterEvent>>> GetAllAsync();
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisasterManagementSystem_Data.Repositories.Interfaces
{
    public interface ITestService
    {
        Task<string> GetTestDataAsync();
    }
}

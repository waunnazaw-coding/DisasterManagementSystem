using DisasterManagementSystem_Data.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisasterManagementSystem_Data.Repositories.Implements
{
    public class TestService : ITestService
    {
        public Task<string> GetTestDataAsync()
        {
            throw new NotImplementedException();
        }
    }
}

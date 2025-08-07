using DisasterManagementSystem_Data.Models;
using DisasterManagementSystem_Data.Repositories.Interfaces;
using DisasterManagementSystem_Services.Models;
using DisasterManagementSystem_Services.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisasterManagementSystem_Services.Services.Implements
{
    public class DisasterEventService:IDisasterEventService
    {
        private readonly IDisasterEventRepository _repository;

        public DisasterEventService(IDisasterEventRepository repository)
        {
            _repository = repository;
        }
        public async Task<Result<IEnumerable<DisasterEvent>>> GetAllAsync()
        {
            var events = await _repository.GetAllAsync();
            return Result<IEnumerable<DisasterEvent>>.Success(events);
        }

    }
}

using DisasterManagementSystem_Data.Models;
using DisasterManagementSystem_Services.Models;
using DisasterManagementSystem_Services.Models.LocationDtos;
using Microsoft.AspNetCore.Mvc;

namespace DisasterManagementSystem_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LocationController : ControllerBase
    {
        private readonly IlocationService _locationService;

        public LocationController(IlocationService locationService)
        {
            _locationService = locationService;
        }

        [HttpGet("{id}")]
        public async Task<IResult> Get(int id)
        {
            var result = await _locationService.GetByIdAsync(id);
            return result.Execute();
        }

        [HttpGet]
        public async Task<IResult> GetAll()
        {
            var result = await _locationService.GetAllAsync();
            return result.Execute();
        }

        [HttpPost]
        public async Task<IResult> Create([FromBody] LocationCreateDto dto)
        {
            var result = await _locationService.AddAsync(dto);
            return result.Execute();
        }

        [HttpPut("{id}")]
        public async Task<IResult> Update(int id, [FromBody] Location model)
        {
            if (id != model.Id)
                return Result<Location>.ValidationError("ID in route and body do not match").Execute();

            var result = await _locationService.UpdateAsync(model);
            return result.Execute();
        }

        [HttpDelete("{id}")]
        public async Task<IResult> Delete(int id)
        {
            var result = await _locationService.DeleteAsync(id);
            return result.Execute();
        }
    }
}

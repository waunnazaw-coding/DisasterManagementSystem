using DisasterManagementSystem_Data.Models;
using DisasterManagementSystem_Data.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DisasterManagementSystem_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AssistanceRequestsController : ControllerBase
    {
        private readonly IAssistanceRequestRepository _repository;

        public AssistanceRequestsController(IAssistanceRequestRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var requests = await _repository.GetAllAsync();
            return Ok(requests);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var request = await _repository.GetByIdAsync(id);
            if (request == null) return NotFound();
            return Ok(request);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] AssistanceRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            request.CreatedAt = DateTime.UtcNow;
            await _repository.AddAsync(request);
            await _repository.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = request.Id }, request);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] AssistanceRequest updatedRequest)
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null) return NotFound();

            existing.SupportType = updatedRequest.SupportType;
            existing.Quantity = updatedRequest.Quantity;
            existing.Unit = updatedRequest.Unit;
            existing.Description = updatedRequest.Description;
            existing.Priority = updatedRequest.Priority;
            existing.Status = updatedRequest.Status;
            existing.ContactName = updatedRequest.ContactName;
            existing.ContactPhone = updatedRequest.ContactPhone;
            existing.Email = updatedRequest.Email;
            existing.DetailedAddress = updatedRequest.DetailedAddress;
            existing.Source = updatedRequest.Source;
            existing.UpdatedAt = DateTime.UtcNow;

            await _repository.UpdateAsync(existing);
            await _repository.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var request = await _repository.GetByIdAsync(id);
            if (request == null) return NotFound();

            await _repository.DeleteAsync(id);
            await _repository.SaveChangesAsync();

            return NoContent();
        }

    }
}

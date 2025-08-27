using DisasterManagementSystem_Services.Models;
using DisasterManagementSystem_Services.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DisasterManagementSystem_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContactController : ControllerBase
    {
        private readonly IContactService _contactService;

        public ContactController(IContactService contactService)
        {
            _contactService = contactService;
        }

        [HttpGet]
        public async Task<IResult> GetAllContacts()
        {
            var result = await _contactService.GetAllContactsAsync();
            return result.Execute();
        }

        [HttpGet("{id}")]
        public async Task<IResult> GetContactById(int id)
        {
            var result = await _contactService.GetContactByIdAsync(id);
            return result.Execute();
        }

        [HttpPost]
        public async Task<IResult> CreateContact([FromBody] ContactDto contactDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return Result<object>.ValidationError(string.Join(", ", errors)).Execute();
            }

            var result = await _contactService.CreateContactAsync(contactDto);
            return result.Execute();
        }

        [HttpPut("{id}")]
        public async Task<IResult> UpdateContact(int id, [FromBody] ContactDto contactDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return Result<object>.ValidationError(string.Join(", ", errors)).Execute();
            }

            var result = await _contactService.UpdateContactAsync(id, contactDto);
            return result.Execute();
        }

        [HttpDelete("{id}")]
        public async Task<IResult> DeleteContact(int id)
        {
            var result = await _contactService.DeleteContactAsync(id);
            return result.Execute();
        }

        [HttpGet("stats")]
        //[Authorize(Roles = "Admin,SysAdmin")]
        public async Task<IResult> GetContactStats()
        {
          
                var result = await _contactService.GetContactStatsAsync();
                return result.Execute();
            
        }
    }
}

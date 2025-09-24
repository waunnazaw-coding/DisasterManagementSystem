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
    public class ContactService : IContactService
    {
        private readonly IContactRepository _contactRepository;

        public ContactService(IContactRepository contactRepository)
        {
            _contactRepository = contactRepository;
        }

        public async Task<Result<ContactDto>> GetContactByIdAsync(int id)
        {
            try
            {
                var contact = await _contactRepository.GetByIdAsync(id);
                if (contact == null)
                    return Result<ContactDto>.NotFoundError("Contact not found");

                var contactDto = MapToDto(contact);
                return Result<ContactDto>.Success(contactDto, "Contact retrieved successfully");
            }
            catch (Exception ex)
            {
                return Result<ContactDto>.Failure($"Error retrieving contact: {ex.Message}");
            }
        }

        public async Task<Result<List<ContactDto>>> GetAllContactsAsync()
        {
            try
            {
                var contacts = await _contactRepository.GetAllAsync();
                var contactDtos = contacts.Select(MapToDto).ToList();

                return Result<List<ContactDto>>.Success(contactDtos, "Contacts retrieved successfully");
            }
            catch (Exception ex)
            {
                return Result<List<ContactDto>>.Failure($"Error retrieving contacts: {ex.Message}");
            }
        }

        public async Task<Result<int>> CreateContactAsync(ContactDto contactDto)
        {
            try
            {
                var contact = MapToEntity(contactDto);
                contact.SubmissionDate = DateTime.Now;

                await _contactRepository.AddAsync(contact);
                return Result<int>.Success(contact.Id, "Contact created successfully");
            }
            catch (Exception ex)
            {
                return Result<int>.Failure($"Error creating contact: {ex.Message}");
            }
        }

        public async Task<Result<bool>> UpdateContactAsync(int id, ContactDto contactDto)
        {
            try
            {
                var contact = await _contactRepository.GetByIdAsync(id);
                if (contact == null)
                    return Result<bool>.NotFoundError("Contact not found");

                // Update properties
                contact.Name = contactDto.Name;
                contact.Phone = contactDto.Phone;
                contact.Email = contactDto.Email;
                contact.Message = contactDto.Message;

                await _contactRepository.UpdateAsync(contact);
                return Result<bool>.Success(true, "Contact updated successfully");
            }
            catch (Exception ex)
            {
                return Result<bool>.Failure($"Error updating contact: {ex.Message}");
            }
        }

        public async Task<Result<bool>> DeleteContactAsync(int id)
        {
            try
            {
                if (!await _contactRepository.ExistsAsync(id))
                    return Result<bool>.NotFoundError("Contact not found");

                await _contactRepository.DeleteAsync(id);
                return Result<bool>.Success(true, "Contact deleted successfully");
            }
            catch (Exception ex)
            {
                return Result<bool>.Failure($"Error deleting contact: {ex.Message}");
            }
        }
        public async Task<Result<ContactStatsDto>> GetContactStatsAsync()
        {
            try
            {
                var contacts = await _contactRepository.GetAllAsync();
                var stats = new ContactStatsDto
                {
                    TotalContacts = contacts.Count,
                    Last30Days = contacts.Count(c => c.SubmissionDate >= DateTime.Now.AddDays(-30)),
                    Last7Days = contacts.Count(c => c.SubmissionDate >= DateTime.Now.AddDays(-7)),
                    Today = contacts.Count(c => c.SubmissionDate.Value.Date == DateTime.Today)
                };

                return Result<ContactStatsDto>.Success(stats, "Contact stats retrieved successfully");
            }
            catch (Exception ex)
            {
                return Result<ContactStatsDto>.Failure($"Error retrieving contact stats: {ex.Message}");
            }
        }

        // Manual mapping methods
        private ContactDto MapToDto(Contact contact)
        {
            return new ContactDto
            {
                Id=contact.Id,
                Name = contact.Name,
                Phone = contact.Phone,
                Email = contact.Email,
                Message = contact.Message,
                SubmissionDate=contact.SubmissionDate
            };
        }

        private Contact MapToEntity(ContactDto contactDto)
        {
            return new Contact
            {
                Id = contactDto.Id,
                Name = contactDto.Name,
                Phone = contactDto.Phone,
                Email = contactDto.Email,
                Message = contactDto.Message,
                SubmissionDate = contactDto.SubmissionDate
            };
        }
    }
}

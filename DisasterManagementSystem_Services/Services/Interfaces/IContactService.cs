using DisasterManagementSystem_Services.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisasterManagementSystem_Services.Services.Interfaces
{
    public interface IContactService
    {
        Task<Result<ContactDto>> GetContactByIdAsync(int id);
        Task<Result<List<ContactDto>>> GetAllContactsAsync();
        Task<Result<int>> CreateContactAsync(ContactDto contactDto);
        Task<Result<bool>> UpdateContactAsync(int id, ContactDto contactDto);
        Task<Result<bool>> DeleteContactAsync(int id);
        Task<Result<ContactStatsDto>> GetContactStatsAsync();
    }
}

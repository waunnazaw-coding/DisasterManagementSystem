using DisasterManagementSystem_Services.Models;
using DisasterManagementSystem_Services.Models.PartnerDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisasterManagementSystem_Services.Services.Interfaces
{
    public interface IPartnerService
    {

        Task<Result<PartnerDTO>> CreatePartnerAsync(PartnerCreateDTO partnerDto);
        Task<Result<PartnerDTO>> GetPartnerAsync(int id);
        Task<Result<List<PartnerDTO>>> GetAllPartnersAsync();
        Task<Result<PartnerDTO>> UpdatePartnerAsync(PartnerUpdateDTO partnerDto);
        Task<Result<bool>> DeletePartnerAsync(int id);
        Task<Result<bool>> UpdatePartnerStatusAsync(int id, string status);

        Task<Result<List<PartnerDTO>>> GetPublicPartnersAsync();
    }
}

using DisasterManagementSystem_Data.Models;
using DisasterManagementSystem_Services.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisasterManagementSystem_Services.Services.Interfaces
{
    public interface IDonationService
    {
        Task<Result<DonationDto>> CreateDonationAsync(CreateDonationDto donationDto, Guid userId);
        Task<Result<List<DonationDto>>> GetAllDonationsAsync();
        Task<Result<List<DonationDto>>> GetUserDonationsAsync(Guid userId);
        Task<Result<DonationDto>> GetDonationByIdAsync(int id);
        //Task<Result<DonationDistribution>> DistributeDonationAsync(DonationDistributionDto distributionDto, Guid distributedBy);
    }
}

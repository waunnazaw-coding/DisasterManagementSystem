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
        Task<Result<DonationDto>> UpdateDonationStatusAsync(int id, string status, Guid updatedBy);
        Task<Result<DonationDto>> UpdateDonationAsync(int id, UpdateDonationDto donationDto, Guid userId);
        Task<Result<bool>> DeleteDonationAsync(int id, Guid userId);
        //Task<Result<DonationDistribution>> DistributeDonationAsync(DonationDistributionDto distributionDto, Guid distributedBy);

        Task<Result<List<DonationDto>>> GetRecentDonationsAsync();
        Task<int> GetTotalPeopleByPhoneAsync();
        Task<decimal?> GetTotalAmountLastYearAsync();
    }
}

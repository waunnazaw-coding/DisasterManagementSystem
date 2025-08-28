using DisasterManagementSystem_Data.Models;
using DisasterManagementSystem_Data.Repositories.Interfaces;
using DisasterManagementSystem_Services.Models;
using DisasterManagementSystem_Services.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DisasterManagementSystem_Services.Services.Implements
{
    public class DonationService : IDonationService
    {
        private readonly IDonationRepository _donationRepository;
        private readonly IUserRepository _userRepository;
        private readonly INotificationService _notificationService;
        private readonly ILogger<DonationService> _logger;

        public DonationService(
         IDonationRepository donationRepository,
         IUserRepository userRepository,
         INotificationService notificationService,
         ILogger<DonationService> logger)
        {
            _donationRepository = donationRepository;
            _userRepository = userRepository;
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task<Result<DonationDto>> CreateDonationAsync(CreateDonationDto donationDto, Guid userId)
        {
            try
            {
                // Validate donation type
                //if (donationDto.Type != "Money" && donationDto.Type != "Item")
                //    return Result<DonationDto>.ValidationError("Donation type must be either 'Money' or 'Item'.");

                //// Validate money donation
                //if (donationDto.Type == "Money")
                //{
                //    if (!donationDto.Amount.HasValue || donationDto.Amount <= 0)
                //        return Result<DonationDto>.ValidationError("Amount is required for money donations and must be greater than 0.");

                //    if (string.IsNullOrEmpty(donationDto.Currency))
                //        return Result<DonationDto>.ValidationError("Currency is required for money donations.");

                //    if (string.IsNullOrEmpty(donationDto.PaymentMethod))
                //        return Result<DonationDto>.ValidationError("Payment method is required for money donations.");

                //    // Validate payment method
                //    var validPaymentMethods = new[] { "KPay", "WavePay", "BankTransfer" };
                //    if (!validPaymentMethods.Contains(donationDto.PaymentMethod))
                //        return Result<DonationDto>.ValidationError("Invalid payment method. Must be KPay, WavePay, or BankTransfer.");

                //    // Reset quantity fields for money donations
                //    donationDto.Quantity = null;
                //    donationDto.Unit = null;
                //}

                // Validate item donation
                //if (donationDto.Type == "Item")
                //{
                //    //if (!donationDto.Quantity.HasValue || donationDto.Quantity <= 0)
                //    //    return Result<DonationDto>.ValidationError("Quantity is required for item donations and must be greater than 0.");

                //    //if (string.IsNullOrEmpty(donationDto.Unit))
                //    //    return Result<DonationDto>.ValidationError("Unit is required for item donations.");

                //    // Reset amount fields for item donations
                //    donationDto.Amount = null;
                //    donationDto.Currency = null;
                //    donationDto.PaymentMethod = null;
                //}




                

                // Validate source type
                var validSourceTypes = new[] { "Personal", "Organization", "NGO", "Anonymous", "Company" };
                if (!validSourceTypes.Contains(donationDto.SourceType))
                    return Result<DonationDto>.ValidationError("Invalid source type.");

                // Get user info
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                    return Result<DonationDto>.NotFoundError("User not found.");

                // Create donation entity
                var donation = new Donation
                {
                    DonorUserId = userId,
                    Name = donationDto.Name,
                    //Type = donationDto.Type,
                    Description = donationDto.Description,
                    //Quantity = donationDto.Quantity,
                    //Unit = donationDto.Unit,
                    Amount = donationDto.Amount,
                    Category = donationDto.Category,
                    Currency = donationDto.Currency,
                    PaymentMethod = donationDto.PaymentMethod,
                    Phone = donationDto.DonorPhoneNumber,
                    DateReceived = DateTime.UtcNow,
                    SourceType = donationDto.SourceType,
                    Status = "Pending"
                };

                // Save to database
                var createdDonation = await _donationRepository.CreateAsync(donation);

                // Notify admins about new donation
                await _notificationService.NotifyAdminsForNewDonation(createdDonation);
                // Create response DTO
                var donationDtoResponse = new DonationDto
                {
                    Id = createdDonation.Id,
                    DonorUserId = createdDonation.DonorUserId,
                    DonorName = user.Name,
                    //Type = createdDonation.Type,
                    Name = createdDonation.Name,
                    Description = createdDonation.Description,
                    Quantity = createdDonation.Quantity,
                    Unit = createdDonation.Unit,
                    Amount = createdDonation.Amount,
                    Currency = createdDonation.Currency,
                    PaymentMethod = createdDonation.PaymentMethod,
                    DonorPhoneNumber = createdDonation.Phone,
                    DateReceived = createdDonation.DateReceived,
                    SourceType = createdDonation.SourceType,
                    Status = createdDonation.Status
                };

                return Result<DonationDto>.Success(donationDtoResponse, "Donation created successfully.");
            }
            catch (Exception ex)
            {
                return Result<DonationDto>.Failure($"Error creating donation: {ex.Message}");
            }
        }

        public async Task<Result<List<DonationDto>>> GetAllDonationsAsync()
        {
            try
            {
                var donations = await _donationRepository.GetAllAsync();
                var donationDtos = donations.Select(d => new DonationDto
                {
                    Id = d.Id,
                    DonorUserId = d.DonorUserId,
                    DonorName = d.DonorUser?.Name,
                    //Type = d.Type,
                    Category = d.Category,
                    Name = d.Name,
                    Description = d.Description,
                    Quantity = d.Quantity,
                    Unit = d.Unit,
                    Amount = d.Amount,
                    
                    Currency = d.Currency,
                    PaymentMethod = d.PaymentMethod,
                    DonorPhoneNumber = d.Phone,
                    DateReceived = d.DateReceived,
                    SourceType = d.SourceType,
                    Status = d.Status
                }).ToList();

                return Result<List<DonationDto>>.Success(donationDtos);
            }
            catch (Exception ex)
            {
                return Result<List<DonationDto>>.Failure($"Error retrieving donations: {ex.Message}");
            }
        }

        public async Task<Result<List<DonationDto>>> GetUserDonationsAsync(Guid userId)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                    return Result<List<DonationDto>>.NotFoundError("User not found.");

                var donations = await _donationRepository.GetByUserIdAsync(userId);
                var donationDtos = donations.Select(d => new DonationDto
                {
                    Id = d.Id,
                    DonorUserId = d.DonorUserId,
                    DonorName = user.Name,
                    //Type = d.Type,
                    Name = d.Name,
                    Category = d.Category,
                    Description = d.Description,
                    Quantity = d.Quantity,
                    Unit = d.Unit,
                    Amount = d.Amount,
                    Currency = d.Currency,
                    PaymentMethod = d.PaymentMethod,
                    DonorPhoneNumber = d.Phone,
                    DateReceived = d.DateReceived,
                    SourceType = d.SourceType,
                    Status = d.Status
                }).ToList();

                return Result<List<DonationDto>>.Success(donationDtos);
            }
            catch (Exception ex)
            {
                return Result<List<DonationDto>>.Failure($"Error retrieving user donations: {ex.Message}");
            }
        }

        public async Task<Result<DonationDto>> GetDonationByIdAsync(int id)
        {
            try
            {
                var donation = await _donationRepository.GetByIdAsync(id);
                if (donation == null)
                    return Result<DonationDto>.NotFoundError("Donation not found.");

                var donationDto = new DonationDto
                {
                    Id = donation.Id,
                    DonorUserId = donation.DonorUserId,
                    DonorName = donation.DonorUser?.Name,
                    //Type = donation.Type,
                    Name = donation.Name,
                    Category = donation.Category,
                    Description = donation.Description,
                    Quantity = donation.Quantity,
                    Unit = donation.Unit,
                    Amount = donation.Amount,
                    Currency = donation.Currency,
                    PaymentMethod = donation.PaymentMethod,
                    DonorPhoneNumber = donation.Phone,
                    DateReceived = donation.DateReceived,
                    SourceType = donation.SourceType,
                    Status = donation.Status
                };

                return Result<DonationDto>.Success(donationDto);
            }
            catch (Exception ex)
            {
                return Result<DonationDto>.Failure($"Error retrieving donation: {ex.Message}");
            }
        }

        public async Task<Result<DonationDto>> UpdateDonationStatusAsync(int id, string status, Guid updatedBy)
        {
            try
            {
                // Validate status
                var validStatuses = new[] { "Verified", "Cancelled", "Distributed", "Pending" };
                if (!validStatuses.Contains(status))
                    return Result<DonationDto>.ValidationError("Status must be 'Verified', 'Cancelled', 'Distributed', or 'Pending'.");

                // Get donation
                var donation = await _donationRepository.GetByIdAsync(id);
                if (donation == null)
                    return Result<DonationDto>.NotFoundError("Donation not found.");

                // Get admin user
                var admin = await _userRepository.GetByIdAsync(updatedBy);
                if (admin == null)
                    return Result<DonationDto>.NotFoundError("Admin user not found.");

                // Update status
                donation.Status = status;

                // Save changes
                await _donationRepository.UpdateAsync(donation);

                // Notify donor about status change
                if (donation.DonorUserId.HasValue && donation.Status != "Pending")
                {
                    await _notificationService.NotifyDonorAboutStatusChange(donation);
                }

                // Create response DTO
                var donationDto = new DonationDto
                {
                    Id = donation.Id,
                    DonorUserId = donation.DonorUserId,
                    DonorName = donation.DonorUser?.Name,
                    //Type = donation.Type,
                    Name = donation.Name,
                    Description = donation.Description,
                    Quantity = donation.Quantity,
                    Unit = donation.Unit,
                    Amount = donation.Amount,
                    Currency = donation.Currency,
                    PaymentMethod = donation.PaymentMethod,
                    DonorPhoneNumber = donation.Phone,
                    DateReceived = donation.DateReceived,
                    SourceType = donation.SourceType,
                    Status = donation.Status
                };

                return Result<DonationDto>.Success(donationDto, $"Donation status updated to {status} successfully.");
            }
            catch (Exception ex)
            {
                return Result<DonationDto>.Failure($"Error updating donation status: {ex.Message}");
            }
        }




        public async Task<Result<DonationDto>> UpdateDonationAsync(int id, UpdateDonationDto donationDto, Guid userId)
        {
            try
            {
                // Get existing donation
                var donation = await _donationRepository.GetByIdAsync(id);
                if (donation == null)
                    return Result<DonationDto>.NotFoundError("Donation not found.");

                // Check if user owns this donation or is admin
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                    return Result<DonationDto>.NotFoundError("User not found.");

                bool isAdmin = user.Role == "SysAdmin,FinancialAdmin,DisasterManagementAdmin";

                if (donation.DonorUserId != userId && !isAdmin)
                    return Result<DonationDto>.ValidationError("You can only edit your own donations.");

                // Check if donation is still pending (for non-admins)
                if (!isAdmin && donation.Status != "Pending")
                    return Result<DonationDto>.ValidationError("Only pending donations can be edited.");

                // Validate source type
                var validSourceTypes = new[] { "Personal", "Organization", "NGO", "Anonymous", "Company" };
                if (!validSourceTypes.Contains(donationDto.SourceType))
                    return Result<DonationDto>.ValidationError("Invalid source type.");

                // Validate payment method
                var validPaymentMethods = new[] { "KPay", "WavePay", "BankTransfer" };
                if (!validPaymentMethods.Contains(donationDto.PaymentMethod))
                    return Result<DonationDto>.ValidationError("Invalid payment method. Must be KPay, WavePay, or BankTransfer.");

                // Update donation fields
                donation.Name = donationDto.Name;
                donation.Description = donationDto.Description;
                donation.SourceType = donationDto.SourceType;
                donation.Phone = donationDto.DonorPhoneNumber;
                donation.Amount = donationDto.Amount;
                donation.Currency = donationDto.Currency;
                donation.PaymentMethod = donationDto.PaymentMethod;
                donation.Category = donationDto.Category;

                // Save changes
                await _donationRepository.UpdateAsync(donation);

                // Create response DTO
                var responseDonationDto = new DonationDto
                {
                    Id = donation.Id,
                    DonorUserId = donation.DonorUserId,
                    DonorName = user.Name,
                    Name = donation.Name,
                    Description = donation.Description,
                    Amount = donation.Amount,
                    Currency = donation.Currency,
                    PaymentMethod = donation.PaymentMethod,
                    DonorPhoneNumber = donation.Phone,
                    DateReceived = donation.DateReceived,
                    SourceType = donation.SourceType,
                    Status = donation.Status,
                    Category = donation.Category
                };

                return Result<DonationDto>.Success(responseDonationDto, "Donation updated successfully.");
            }
            catch (Exception ex)
            {
                return Result<DonationDto>.Failure($"Error updating donation: {ex.Message}");
            }
        }

        public async Task<Result<bool>> DeleteDonationAsync(int id, Guid userId)
        {
            try
            {
                // Get existing donation
                var donation = await _donationRepository.GetByIdAsync(id);
                if (donation == null)
                    return Result<bool>.NotFoundError("Donation not found.");

                // Check if user owns this donation or is admin
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                    return Result<bool>.NotFoundError("User not found.");

                bool isAdmin = user.Role == "Admin";

                if (donation.DonorUserId != userId && !isAdmin)
                    return Result<bool>.ValidationError("You can only delete your own donations.");

                // Check if donation is still pending (for non-admins)
                if (!isAdmin && donation.Status != "Pending")
                    return Result<bool>.ValidationError("Only pending donations can be deleted.");

                // Delete donation
                var result = await _donationRepository.DeleteAsync(id);
                if (!result)
                    return Result<bool>.NotFoundError("Donation not found.");

                return Result<bool>.Success(true, "Donation deleted successfully.");
            }
            catch (Exception ex)
            {
                return Result<bool>.Failure($"Error deleting donation: {ex.Message}");
            }
        }
        // In DonationService.cs
        public async Task<Result<List<DonationDto>>> GetRecentDonationsAsync()
        {
            try
            {
                var donations = await _donationRepository.GetRecentAsync();
                var donationDtos = donations.Select(d => new DonationDto
                {
                    Id = d.Id,
                    DonorUserId = d.DonorUserId,
                    DonorName = d.DonorUser?.Name,
                    //Type = d.Type,
                    Name = d.Name,
                    Description = d.Description,
                    Quantity = d.Quantity,
                    Unit = d.Unit,
                    Amount = d.Amount,
                    Currency = d.Currency,
                    PaymentMethod = d.PaymentMethod,
                    DonorPhoneNumber = d.Phone,
                    DateReceived = d.DateReceived,
                    SourceType = d.SourceType,
                    Status = d.Status
                }).ToList();

                return Result<List<DonationDto>>.Success(donationDtos);
            }
            catch (Exception ex)
            {
                return Result<List<DonationDto>>.Failure($"Error retrieving recent donations: {ex.Message}");
            }
        }

        public async Task<int> GetTotalPeopleByPhoneAsync()
        {
            return await _donationRepository.GetTotalPeopleByPhoneAsync();
        }

        public async Task<decimal?> GetTotalAmountLastYearAsync()
        {
            return await _donationRepository.GetTotalAmountLastYearAsync();
        }
    }
}

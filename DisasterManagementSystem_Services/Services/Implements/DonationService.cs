using DisasterManagementSystem_Data.Models;
using DisasterManagementSystem_Data.Repositories.Interfaces;
using DisasterManagementSystem_Services.Models;
using DisasterManagementSystem_Services.Services.Interfaces;
using Microsoft.AspNetCore.Mvc.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisasterManagementSystem_Services.Services.Implements
{
    public class DonationService:IDonationService
    {
        private readonly IDonationRepository _donationRepository;
        private readonly IUserRepository _userRepository;
        public DonationService(IDonationRepository donationRepository, IUserRepository userRepository)
        {
            _donationRepository = donationRepository;
            _userRepository = userRepository;
        }
        public async Task<Result<DonationDto>> CreateDonationAsync(CreateDonationDto donationDto, Guid userId)
        {
            try
            {
                // Validate donation type
                if (donationDto.Type != "Money" && donationDto.Type != "Item")
                    return Result<DonationDto>.ValidationError("Donation type must be either 'Money' or 'Item'.");

                // Validate money donation
                if (donationDto.Type == "Money")
                {
                    if (!donationDto.Amount.HasValue || donationDto.Amount <= 0)
                        return Result<DonationDto>.ValidationError("Amount is required for money donations and must be greater than 0.");

                    // Reset quantity fields for money donations
                    donationDto.Quantity = null;
                    donationDto.Unit = null;
                }

                // Validate item donation
                if (donationDto.Type == "Item")
                {
                    if (!donationDto.Quantity.HasValue || donationDto.Quantity <= 0)
                        return Result<DonationDto>.ValidationError("Quantity is required for item donations and must be greater than 0.");

                    // Reset amount fields for item donations
                    donationDto.Amount = null;
                    donationDto.Currency = null;
                }

                // Get user info
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                    return Result<DonationDto>.NotFoundError("User not found.");

                // Create donation entity
                var donation = new Donation
                {
                    DonorUserId = userId,
                    Name = donationDto.Name,
                    Type = donationDto.Type,
                    Description = donationDto.Description,
                    Quantity = donationDto.Quantity,
                    Unit = donationDto.Unit,
                    Amount = donationDto.Amount,
                    Currency = donationDto.Currency,
                    DateReceived = DateTime.UtcNow,
                    SourceType = donationDto.SourceType,
                    Status = "Pending"
                };

                // Save to database
                var createdDonation = await _donationRepository.CreateAsync(donation);

                // Create response DTO
                var donationDtoResponse = new DonationDto
                {
                    Id = createdDonation.Id,
                    DonorUserId = createdDonation.DonorUserId,
                    DonorName = user.Name,
                    Type = createdDonation.Type,
                    Name = createdDonation.Name,
                    Description = createdDonation.Description,
                    Quantity = createdDonation.Quantity,
                    Unit = createdDonation.Unit,
                    Amount = createdDonation.Amount,
                    Currency = createdDonation.Currency,
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
                    Type = d.Type,
                    Name = d.Name,
                    Description = d.Description,
                    Quantity = d.Quantity,
                    Unit = d.Unit,
                    Amount = d.Amount,
                    Currency = d.Currency,
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
                    DonorName = user.Name, // Use the same name for all donations from this user
                    Type = d.Type,
                    Name = d.Name,
                    Description = d.Description,
                    Quantity = d.Quantity,
                    Unit = d.Unit,
                    Amount = d.Amount,
                    Currency = d.Currency,
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
                    Type = donation.Type,
                    Name = donation.Name,
                    Description = donation.Description,
                    Quantity = donation.Quantity,
                    Unit = donation.Unit,
                    Amount = donation.Amount,
                    Currency = donation.Currency,
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

        //public async Task<Result<DonationDistribution>> DistributeDonationAsync(DonationDistributionDto distributionDto, Guid distributedBy)
        //{
        //    try
        //    {
        //        var donation = await _donationRepository.GetByIdAsync(distributionDto.DonationId);
        //        if (donation == null)
        //            return Result<DonationDistribution>.NotFoundError("Donation not found.");

        //        // Validate distribution based on donation type
        //        if (donation.Type == "Money" && !distributionDto.Amount.HasValue)
        //            return Result<DonationDistribution>.ValidationError("Amount is required for money distribution.");

        //        if (donation.Type == "Item" && !distributionDto.Quantity.HasValue)
        //            return Result<DonationDistribution>.ValidationError("Quantity is required for item distribution.");

        //        // Check if either assistance request or relief team is specified
        //        if (!distributionDto.AssistanceRequestId.HasValue && !distributionDto.BeneficiaryReliefTeamId.HasValue)
        //            return Result<DonationDistribution>.ValidationError("Either Assistance Request or Relief Team must be specified.");

        //        // Check assistance request if specified
        //        if (distributionDto.AssistanceRequestId.HasValue)
        //        {
        //           // var assistanceRequest = await _assistanceRequestRepository.GetByIdAsync(distributionDto.AssistanceRequestId.Value);
        //          //  if (assistanceRequest == null)
        //                return Result<DonationDistribution>.NotFoundError("Assistance Request not found.");
        //        }

        //        // Check relief team if specified
        //        if (distributionDto.BeneficiaryReliefTeamId.HasValue)
        //        {
        //           // var reliefTeam = await _reliefTeamRepository.GetByIdAsync(distributionDto.BeneficiaryReliefTeamId.Value);
        //           // if (reliefTeam == null)
        //                return Result<DonationDistribution>.NotFoundError("Relief Team not found.");
        //        }

        //        // Create distribution record
        //        var distribution = new DonationDistribution
        //        {
        //            DonationId = distributionDto.DonationId,
        //            AssistanceRequestId = distributionDto.AssistanceRequestId,
        //            BeneficiaryReliefTeamId = distributionDto.BeneficiaryReliefTeamId,
        //            Quantity = distributionDto.Quantity,
        //            Amount = distributionDto.Amount,
        //            DateDistributed = DateTime.UtcNow,
        //            Status = "Distributed",
        //            DistributedBy = distributedBy,
        //            DistributionNotes = distributionDto.DistributionNotes
        //        };

        //        // Update donation status
        //        donation.Status = "Distributed";
        //        await _donationRepository.UpdateAsync(donation);

        //        return Result<DonationDistribution>.Success(distribution, "Donation distributed successfully.");
        //    }
        //    catch (Exception ex)
        //    {
        //        return Result<DonationDistribution>.Failure($"Error distributing donation: {ex.Message}");
        //    }
        //}
    }
}

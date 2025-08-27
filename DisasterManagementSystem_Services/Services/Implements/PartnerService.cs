using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using DisasterManagementSystem_Data.Models;
using DisasterManagementSystem_Data.Repositories.Interfaces;
using DisasterManagementSystem_Services.Models;
using DisasterManagementSystem_Services.Models.PartnerDtos;
using DisasterManagementSystem_Services.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DisasterManagementSystem_Services.Services.Implements
{
    public class PartnerService : IPartnerService
    {
        private readonly IPartnerRepository _partnerRepository;
        private readonly Cloudinary _cloudinary;

        public PartnerService(
            IPartnerRepository partnerRepository,
            IOptions<CloudinarySettings> config)
        {
            _partnerRepository = partnerRepository;

            var account = new Account(
                config.Value.CloudName,
                config.Value.ApiKey,
                config.Value.ApiSecret);
            _cloudinary = new Cloudinary(account);
        }

        public async Task<Result<PartnerDTO>> CreatePartnerAsync(PartnerCreateDTO partnerDto)
        {
            try
            {
                var partner = new Partner
                {
                    Name = partnerDto.Name,
                    ContactName = partnerDto.ContactName,
                    Email = partnerDto.Email,
                    Phone = partnerDto.Phone,
                    Address = partnerDto.Address,
                    Website = partnerDto.Website,
                    Notes = partnerDto.Notes,
                    IsPublic = partnerDto.IsPublic,
                    Status = "Active",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                // Handle logo upload if provided
                if (partnerDto.LogoFile != null && partnerDto.LogoFile.Length > 0)
                {
                    var uploadResult = await UploadLogoToCloudinary(partnerDto.LogoFile);
                    if (uploadResult.IsSuccess)
                    {
                        partner.LogoUrl = uploadResult.Data.Url;
                        partner.LogoPublicId = uploadResult.Data.PublicId;
                        partner.LogoFileType = partnerDto.LogoFile.ContentType;
                        partner.LogoFileSize = partnerDto.LogoFile.Length;
                    }
                    else
                    {
                        return Result<PartnerDTO>.Failure(uploadResult.Message);
                    }
                }

                var createdPartner = await _partnerRepository.AddAsync(partner);
                return Result<PartnerDTO>.Success(MapToDto(createdPartner), "Partner created successfully");
            }
            catch (Exception ex)
            {
                return Result<PartnerDTO>.Failure($"Error creating partner: {ex.Message}");
            }
        }

        public async Task<Result<PartnerDTO>> GetPartnerAsync(int id)
        {
            try
            {
                var partner = await _partnerRepository.GetByIdAsync(id);
                if (partner == null)
                    return Result<PartnerDTO>.NotFoundError("Partner not found");

                return Result<PartnerDTO>.Success(MapToDto(partner));
            }
            catch (Exception ex)
            {
                return Result<PartnerDTO>.Failure($"Error retrieving partner: {ex.Message}");
            }
        }

        public async Task<Result<List<PartnerDTO>>> GetAllPartnersAsync()
        {
            try
            {
                var partners = await _partnerRepository.GetAllAsync();
                var partnerDtos = partners.Select(MapToDto).ToList();
                return Result<List<PartnerDTO>>.Success(partnerDtos);
            }
            catch (Exception ex)
            {
                return Result<List<PartnerDTO>>.Failure($"Error retrieving partners: {ex.Message}");
            }
        }

        public async Task<Result<PartnerDTO>> UpdatePartnerAsync(PartnerUpdateDTO partnerDto)
        {
            try
            {
                var existingPartner = await _partnerRepository.GetByIdAsync(partnerDto.Id);
                if (existingPartner == null)
                    return Result<PartnerDTO>.NotFoundError("Partner not found");

                // Update basic properties
                existingPartner.Name = partnerDto.Name;
                existingPartner.ContactName = partnerDto.ContactName;
                existingPartner.Email = partnerDto.Email;
                existingPartner.Phone = partnerDto.Phone;
                existingPartner.Address = partnerDto.Address;
                existingPartner.Website = partnerDto.Website;
                existingPartner.Notes = partnerDto.Notes;
                existingPartner.IsPublic = partnerDto.IsPublic;
                existingPartner.Status = partnerDto.Status;
                existingPartner.UpdatedAt = DateTime.UtcNow;

                // Handle logo removal
                if (partnerDto.RemoveLogo && !string.IsNullOrEmpty(existingPartner.LogoPublicId))
                {
                    await DeleteLogoFromCloudinary(existingPartner.LogoPublicId);
                    existingPartner.LogoUrl = null;
                    existingPartner.LogoPublicId = null;
                    existingPartner.LogoFileType = null;
                    existingPartner.LogoFileSize = null;
                }

                // Handle new logo upload
                if (partnerDto.LogoFile != null && partnerDto.LogoFile.Length > 0)
                {
                    // Remove old logo if exists
                    if (!string.IsNullOrEmpty(existingPartner.LogoPublicId))
                    {
                        await DeleteLogoFromCloudinary(existingPartner.LogoPublicId);
                    }

                    var uploadResult = await UploadLogoToCloudinary(partnerDto.LogoFile);
                    if (uploadResult.IsSuccess)
                    {
                        existingPartner.LogoUrl = uploadResult.Data.Url;
                        existingPartner.LogoPublicId = uploadResult.Data.PublicId;
                        existingPartner.LogoFileType = partnerDto.LogoFile.ContentType;
                        existingPartner.LogoFileSize = partnerDto.LogoFile.Length;
                    }
                    else
                    {
                        return Result<PartnerDTO>.Failure(uploadResult.Message);
                    }
                }

                var updatedPartner = await _partnerRepository.UpdateAsync(existingPartner);
                return Result<PartnerDTO>.Success(MapToDto(updatedPartner), "Partner updated successfully");
            }
            catch (Exception ex)
            {
                return Result<PartnerDTO>.Failure($"Error updating partner: {ex.Message}");
            }
        }

        public async Task<Result<bool>> DeletePartnerAsync(int id)
        {
            try
            {
                var partner = await _partnerRepository.GetByIdAsync(id);
                if (partner == null)
                    return Result<bool>.NotFoundError("Partner not found");

                // Delete logo if exists
                if (!string.IsNullOrEmpty(partner.LogoPublicId))
                {
                    await DeleteLogoFromCloudinary(partner.LogoPublicId);
                }

                var success = await _partnerRepository.DeleteAsync(id);
                return success
                    ? Result<bool>.Success(true, "Partner deleted successfully")
                    : Result<bool>.Failure("Failed to delete partner");
            }
            catch (Exception ex)
            {
                return Result<bool>.Failure($"Error deleting partner: {ex.Message}");
            }
        }

        public async Task<Result<bool>> UpdatePartnerStatusAsync(int id, string status)
        {
            try
            {
                var partner = await _partnerRepository.GetByIdAsync(id);
                if (partner == null)
                    return Result<bool>.NotFoundError("Partner not found");

                partner.Status = status;
                partner.UpdatedAt = DateTime.UtcNow;

                await _partnerRepository.UpdateAsync(partner);
                return Result<bool>.Success(true, "Partner status updated successfully");
            }
            catch (Exception ex)
            {
                return Result<bool>.Failure($"Error updating partner status: {ex.Message}");
            }
        }

        private async Task<Result<CloudinaryUploadResult>> UploadLogoToCloudinary(IFormFile file)
        {
            try
            {
                if (file.Length == 0 || !file.ContentType.StartsWith("image/"))
                    return Result<CloudinaryUploadResult>.ValidationError("Invalid image file");

                using var stream = file.OpenReadStream();
                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(file.FileName, stream),
                    UseFilename = true,
                    UniqueFilename = true,
                    Overwrite = false,
                    Folder = "partner_logos",
                    Transformation = new Transformation().Width(300).Height(300).Crop("fill")
                };

                var uploadResult = await _cloudinary.UploadAsync(uploadParams);

                if (uploadResult.StatusCode != System.Net.HttpStatusCode.OK || uploadResult.SecureUrl == null)
                    return Result<CloudinaryUploadResult>.Failure("Failed to upload logo to Cloudinary");

                return Result<CloudinaryUploadResult>.Success(new CloudinaryUploadResult
                {
                    Url = uploadResult.SecureUrl.AbsoluteUri,
                    PublicId = uploadResult.PublicId
                });
            }
            catch (Exception ex)
            {
                return Result<CloudinaryUploadResult>.Failure($"Error uploading logo: {ex.Message}");
            }
        }

        private async Task DeleteLogoFromCloudinary(string publicId)
        {
            try
            {
                var deleteParams = new DeletionParams(publicId);
                await _cloudinary.DestroyAsync(deleteParams);
            }
            catch
            {
                // Log error but don't throw to avoid disrupting the main operation
            }
        }

        private PartnerDTO MapToDto(Partner partner)
        {
            return new PartnerDTO
            {
                Id = partner.Id,
                Name = partner.Name,
                ContactName = partner.ContactName,
                Email = partner.Email,
                Phone = partner.Phone,
                Address = partner.Address,
                Website = partner.Website,
                Notes = partner.Notes,
                IsPublic = partner.IsPublic,
                Status = partner.Status,
                CreatedAt = partner.CreatedAt,
                UpdatedAt = partner.UpdatedAt,
                LogoUrl = partner.LogoUrl
            };
        }

        public async Task<Result<List<PartnerDTO>>> GetPublicPartnersAsync()
        {
            try
            {
                var partners = await _partnerRepository.GetPublicPartnersAsync();
                var partnerDtos = partners.Select(MapToDto).ToList();
                return Result<List<PartnerDTO>>.Success(partnerDtos);
            }
            catch (Exception ex)
            {
                return Result<List<PartnerDTO>>.Failure($"Error retrieving public partners: {ex.Message}");
            }
        }
    }

    // Helper class for Cloudinary upload results
    public class CloudinaryUploadResult
    {
        public string Url { get; set; }
        public string PublicId { get; set; }
    }
}
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using DisasterManagementSystem_Data.Models;
using DisasterManagementSystem_Data.Repositories.Interfaces;
using DisasterManagementSystem_Services.Models;
using DisasterManagementSystem_Services.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DisasterManagementSystem_Services.Services.Implements
{
    public class ReportPhotoService : IReportPhotoService
    {
        private readonly IReportPhotoRepository _photoRepository;
        private readonly Cloudinary _cloudinary;

        public ReportPhotoService(
            IReportPhotoRepository photoRepository,
            IOptions<CloudinarySettings> config)
        {
            _photoRepository = photoRepository;

            var account = new Account(
                config.Value.CloudName,
                config.Value.ApiKey,
                config.Value.ApiSecret);

            _cloudinary = new Cloudinary(account);
        }

        public async Task<Result<List<UploadPhotoResultDTO>>> UploadReportPhotosAsync(int disasterReportId, IFormFile[] files)
        {
            try
            {
                if (files == null || files.Length == 0)
                    return Result<List<UploadPhotoResultDTO>>.ValidationError("No files provided");

                var uploadedPhotos = new List<UploadPhotoResultDTO>();

                foreach (var file in files)
                {
                    if (file.Length == 0)
                        continue;

                    if (!file.ContentType.StartsWith("image/"))
                        continue;

                    var uploadResult = await UploadToCloudinary(file);

                    var photo = new ReportPhoto
                    {
                        DisasterReportId = disasterReportId,
                        DisasterEventId = null,
                        FilePath = uploadResult.SecureUrl.AbsoluteUri,
                        FileType = file.ContentType,
                        FileSize = file.Length,
                        UploadedAt = DateTime.UtcNow
                    };

                    var savedPhoto = await _photoRepository.AddAsync(photo);

                    uploadedPhotos.Add(new UploadPhotoResultDTO
                    {
                        Id = savedPhoto.Id,
                        FilePath = savedPhoto.FilePath,
                        FileType = savedPhoto.FileType,
                        FileSize = savedPhoto.FileSize,
                        UploadedAt = savedPhoto.UploadedAt
                    });
                }

                if (uploadedPhotos.Count == 0)
                    return Result<List<UploadPhotoResultDTO>>.ValidationError("No valid images were uploaded");

                return Result<List<UploadPhotoResultDTO>>.Success(uploadedPhotos);
            }
            catch (Exception ex)
            {
                return Result<List<UploadPhotoResultDTO>>.Failure($"Error uploading photos: {ex.Message}");
            }
        }

        public async Task<Result<List<UploadPhotoResultDTO>>> UploadEventPhotosAsync(int disasterEventId, IFormFile[] files)
        {
            try
            {
                if (files == null || files.Length == 0)
                    return Result<List<UploadPhotoResultDTO>>.ValidationError("No files provided");

                var uploadedPhotos = new List<UploadPhotoResultDTO>();

                foreach (var file in files)
                {
                    if (file.Length == 0)
                        continue;

                    if (!file.ContentType.StartsWith("image/"))
                        continue;

                    var uploadResult = await UploadToCloudinary(file);

                    var photo = new ReportPhoto
                    {
                        DisasterEventId = disasterEventId,
                        DisasterReportId = null,
                        FilePath = uploadResult.SecureUrl.AbsoluteUri,
                        FileType = file.ContentType,
                        FileSize = file.Length,
                        UploadedAt = DateTime.UtcNow
                    };

                    var savedPhoto = await _photoRepository.AddAsync(photo);

                    uploadedPhotos.Add(new UploadPhotoResultDTO
                    {
                        Id = savedPhoto.Id,
                        FilePath = savedPhoto.FilePath,
                        FileType = savedPhoto.FileType,
                        FileSize = savedPhoto.FileSize,
                        UploadedAt = savedPhoto.UploadedAt
                    });
                }

                if (uploadedPhotos.Count == 0)
                    return Result<List<UploadPhotoResultDTO>>.ValidationError("No valid images were uploaded");

                return Result<List<UploadPhotoResultDTO>>.Success(uploadedPhotos);
            }
            catch (Exception ex)
            {
                return Result<List<UploadPhotoResultDTO>>.Failure($"Error uploading photos: {ex.Message}");
            }
        }

        public async Task<Result<UploadPhotoResultDTO>> UpdatePhotoAsync(int photoId, IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return Result<UploadPhotoResultDTO>.ValidationError("No file provided");

                if (!file.ContentType.StartsWith("image/"))
                    return Result<UploadPhotoResultDTO>.ValidationError("Only image files are allowed");

                var existingPhoto = await _photoRepository.GetByIdAsync(photoId);
                if (existingPhoto == null)
                    return Result<UploadPhotoResultDTO>.NotFoundError("Photo not found");

                // Upload new version to Cloudinary
                var uploadResult = await UploadToCloudinary(file);

                // Update photo entity
                var updatedPhoto = new ReportPhoto
                {
                    Id = photoId,
                    DisasterReportId = existingPhoto.DisasterReportId,
                    DisasterEventId = existingPhoto.DisasterEventId,
                    FilePath = uploadResult.SecureUrl.AbsoluteUri,
                    FileType = file.ContentType,
                    FileSize = file.Length,
                    UploadedAt = DateTime.UtcNow
                };

                var result = await _photoRepository.UpdateAsync(updatedPhoto);
                if (result == null)
                    return Result<UploadPhotoResultDTO>.Failure("Failed to update photo");

                // Delete old file from Cloudinary
                await DeleteFromCloudinary(existingPhoto.FilePath);

                return Result<UploadPhotoResultDTO>.Success(new UploadPhotoResultDTO
                {
                    Id = result.Id,
                    FilePath = result.FilePath,
                    FileType = result.FileType,
                    FileSize = result.FileSize,
                    UploadedAt = result.UploadedAt
                });
            }
            catch (Exception ex)
            {
                return Result<UploadPhotoResultDTO>.Failure($"Error updating photo: {ex.Message}");
            }
        }

        public async Task<Result<List<UploadPhotoResultDTO>>> GetPhotosByReportIdAsync(int reportId)
        {
            try
            {
                var photos = await _photoRepository.GetByReportIdAsync(reportId);
                var result = photos.Select(p => new UploadPhotoResultDTO
                {
                    Id = p.Id,
                    FilePath = p.FilePath,
                    FileType = p.FileType,
                    FileSize = p.FileSize,
                    UploadedAt = p.UploadedAt
                }).ToList();

                return Result<List<UploadPhotoResultDTO>>.Success(result);
            }
            catch (Exception ex)
            {
                return Result<List<UploadPhotoResultDTO>>.Failure($"Error retrieving photos: {ex.Message}");
            }
        }

        public async Task<Result<List<UploadPhotoResultDTO>>> GetPhotosByEventIdAsync(int eventId)
        {
            try
            {
                var photos = await _photoRepository.GetByEventIdAsync(eventId);
                var result = photos.Select(p => new UploadPhotoResultDTO
                {
                    Id = p.Id,
                    FilePath = p.FilePath,
                    FileType = p.FileType,
                    FileSize = p.FileSize,
                    UploadedAt = p.UploadedAt
                }).ToList();

                return Result<List<UploadPhotoResultDTO>>.Success(result);
            }
            catch (Exception ex)
            {
                return Result<List<UploadPhotoResultDTO>>.Failure($"Error retrieving photos: {ex.Message}");
            }
        }

        public async Task<Result<UploadPhotoResultDTO>> GetPhotoByIdAsync(int photoId)
        {
            try
            {
                var photo = await _photoRepository.GetByIdAsync(photoId);
                if (photo == null)
                    return Result<UploadPhotoResultDTO>.NotFoundError("Photo not found");

                return Result<UploadPhotoResultDTO>.Success(new UploadPhotoResultDTO
                {
                    Id = photo.Id,
                    FilePath = photo.FilePath,
                    FileType = photo.FileType,
                    FileSize = photo.FileSize,
                    UploadedAt = photo.UploadedAt
                });
            }
            catch (Exception ex)
            {
                return Result<UploadPhotoResultDTO>.Failure($"Error retrieving photo: {ex.Message}");
            }
        }

        public async Task<Result<bool>> DeletePhotoAsync(int photoId)
        {
            try
            {
                var photo = await _photoRepository.GetByIdAsync(photoId);
                if (photo == null)
                    return Result<bool>.NotFoundError("Photo not found");

                // Delete from Cloudinary first
                await DeleteFromCloudinary(photo.FilePath);

                // Then delete from database
                var success = await _photoRepository.DeleteAsync(photoId);
                return success
                    ? Result<bool>.Success(true, "Photo deleted successfully")
                    : Result<bool>.Failure("Failed to delete photo");
            }
            catch (Exception ex)
            {
                return Result<bool>.Failure($"Error deleting photo: {ex.Message}");
            }
        }

        private async Task<ImageUploadResult> UploadToCloudinary(IFormFile file)
        {
            await using var stream = file.OpenReadStream();
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                UseFilename = true,
                UniqueFilename = true, // Make sure it does not overwrite another file
                Overwrite = false,
                Folder = "disaster_photos", // OPTIONAL: keep files organized
                Transformation = new Transformation().Quality("auto").FetchFormat("auto")
            };

            var result = await _cloudinary.UploadAsync(uploadParams);

            Console.WriteLine($"Cloudinary Upload:");
            Console.WriteLine($"  StatusCode: {result.StatusCode}");
            Console.WriteLine($"  SecureUrl: {result.SecureUrl}");
            Console.WriteLine($"  Url: {result.Url}");
            Console.WriteLine($"  PublicId: {result.PublicId}");
            Console.WriteLine($"  Error: {result.Error?.Message}");

            if (result.StatusCode != System.Net.HttpStatusCode.OK || result.SecureUrl == null)
            {
                throw new Exception($"Cloudinary upload failed: {result.Error?.Message ?? "Unknown error"}");
            }

            return result;
        }


        private async Task DeleteFromCloudinary(string fileUrl)
        {
            if (string.IsNullOrEmpty(fileUrl)) return;

            var publicId = GetPublicIdFromUrl(fileUrl);
            if (!string.IsNullOrEmpty(publicId))
            {
                var deleteParams = new DeletionParams(publicId);
                await _cloudinary.DestroyAsync(deleteParams);
            }
        }

        private string? GetPublicIdFromUrl(string url)
        {
            try
            {
                var uri = new Uri(url);
                var segments = uri.Segments;
                if (segments.Length < 3) return null;

                var publicIdWithExtension = segments[^1];
                return publicIdWithExtension.Split('.')[0];
            }
            catch
            {
                return null;
            }
        }
    }
}
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
using System.Text;
using System.Threading.Tasks;

namespace DisasterManagementSystem_Services.Services.Implements
{
    public class ReportPhotoService : IReportPhotoService
    {
        private readonly IReportPhotoRepository _photoRepository;
        private readonly Cloudinary _cloudinary;

        public ReportPhotoService(IReportPhotoRepository photoRepository, IOptions<CloudinarySettings> config)
        {
            _photoRepository = photoRepository;

            var acc = new Account(config.Value.CloudName, config.Value.ApiKey, config.Value.ApiSecret);
            _cloudinary = new Cloudinary(acc);
        }

        public async Task<Result<List<UploadPhotoResultDTO>>> UploadReportPhotosAsync(int disasterReportId, IFormFile[] files)
        {
            var uploadedPhotos = new List<UploadPhotoResultDTO>();

            foreach (var file in files)
            {
                await using var stream = file.OpenReadStream();
                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(file.FileName, stream)
                };

                var uploadResult = await _cloudinary.UploadAsync(uploadParams);

                var photo = new ReportPhoto
                {
                    DisasterReportId = disasterReportId, // Only set this one
                   DisasterEventId = null,             // Explicitly set to null
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

            return Result<List<UploadPhotoResultDTO>>.Success(uploadedPhotos);
        }

        public async Task<Result<List<UploadPhotoResultDTO>>> UploadEventPhotosAsync(int disasterEventId, IFormFile[] files)
        {

            var uploadedPhotos = new List<UploadPhotoResultDTO>();

            foreach (var file in files)
            {
                await using var stream = file.OpenReadStream();
                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(file.FileName, stream)
                };

                var uploadResult = await _cloudinary.UploadAsync(uploadParams);

                var photo = new ReportPhoto
                {
                    DisasterEventId = disasterEventId,
                    DisasterReportId = null,   // Add this line!
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

            return Result<List<UploadPhotoResultDTO>>.Success(uploadedPhotos);
        }
    }
    
}

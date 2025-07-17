using CloudinaryDotNet;
using DisasterManagementSystem_Data.Models;
using DisasterManagementSystem_Data.Repositories.Interfaces;
using DisasterManagementSystem_Services.Models;
using DisasterManagementSystem_Services.Services.Implements;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisasterManagementSystem_Testing.Service
{
    public class ReportPhotoServiceTest
    {
        private readonly Mock<IReportPhotoRepository> _photoRepoMock;
        private readonly Mock<Cloudinary> _cloudinaryMock;
        private readonly ReportPhotoService _service;

        public ReportPhotoServiceTest()
        {
            _photoRepoMock = new Mock<IReportPhotoRepository>();

            // Create fake Cloudinary config
            var config = Options.Create(new CloudinarySettings
            {
                CloudName = "test-cloud",
                ApiKey = "test-key",
                ApiSecret = "test-secret"
            });

            // Setup real Cloudinary with fake config
            _cloudinaryMock = new Mock<Cloudinary>(new Account(config.Value.CloudName, config.Value.ApiKey, config.Value.ApiSecret));

            _service = new ReportPhotoService(
                _photoRepoMock.Object,
                config
            );
        }

        [Fact]
        public async Task UploadReportPhotosAsync_ShouldReturnValidationError_WhenFilesAreNull()
        {
            // Act
            var result = await _service.UploadReportPhotosAsync(1, null);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("No files provided", result.Message);
        }

        [Fact]
        public async Task GetPhotosByReportIdAsync_ShouldReturnPhotos()
        {
            // Arrange
            var photos = new List<ReportPhoto>
            {
                new ReportPhoto { Id = 1, FilePath = "url1", FileType = "image/jpeg", FileSize = 123, UploadedAt = DateTime.UtcNow },
                new ReportPhoto { Id = 2, FilePath = "url2", FileType = "image/png", FileSize = 456, UploadedAt = DateTime.UtcNow }
            };
            _photoRepoMock.Setup(r => r.GetByReportIdAsync(1)).ReturnsAsync(photos);

            // Act
            var result = await _service.GetPhotosByReportIdAsync(1);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(2, result.Data.Count);
        }

        [Fact]
        public async Task GetPhotoByIdAsync_ShouldReturnNotFound_WhenNotExist()
        {
            // Arrange
            _photoRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((ReportPhoto)null);

            // Act
            var result = await _service.GetPhotoByIdAsync(1);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("Photo not found", result.Message);
        }
        [Fact]
        public async Task DeletePhotoAsync_ShouldReturnNotFound_WhenNotExist()
        {
            // Arrange
            _photoRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((ReportPhoto)null);

            // Act
            var result = await _service.DeletePhotoAsync(1);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("Photo not found", result.Message);
        }

        [Fact]
        public async Task DeletePhotoAsync_ShouldReturnSuccess_WhenDeleted()
        {
            // Arrange
            var photo = new ReportPhoto { Id = 1, FilePath = "https://res.cloudinary.com/demo/image/upload/v1234/photo.jpg" };
            _photoRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(photo);
            _photoRepoMock.Setup(r => r.DeleteAsync(1)).ReturnsAsync(true);

            // Act
            var result = await _service.DeletePhotoAsync(1);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("Photo deleted successfully", result.Message);
        }

    }
}

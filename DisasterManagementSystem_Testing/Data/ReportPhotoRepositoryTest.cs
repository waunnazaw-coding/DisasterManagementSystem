using DisasterManagementSystem_Data.Models;
using DisasterManagementSystem_Data.Repositories.Implements;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisasterManagementSystem_Testing.Data
{
    public class ReportPhotoRepositoryTest
    {
        private readonly AppDbContext _dbContext;
        private readonly ReportPhotoRepository _repository;
        public ReportPhotoRepositoryTest()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "ReportPhotoTestDb_" + Guid.NewGuid())
                .Options;

            _dbContext = new AppDbContext(options);
            _repository = new ReportPhotoRepository(_dbContext);
        }
        [Fact]
        public async Task AddAsync_ShouldAddPhoto()
        {
            // Arrange
            var photo = new ReportPhoto
            {
                FilePath = "photos/test.jpg",
                FileType = "image/jpeg",
                FileSize = 1024,
                DisasterReportId = 1
            };

            // Act
            var result = await _repository.AddAsync(photo);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Id > 0);
            Assert.Single(_dbContext.ReportPhotos);
        }


        [Fact]
        public async Task UpdateAsync_ShouldUpdateExistingPhoto()
        {
            // Arrange
            var originalPhoto = new ReportPhoto
            {
                FilePath = "photos/old.jpg",
                FileType = "image/jpeg",
                FileSize = 1024,
                DisasterReportId = 1
            };
            _dbContext.ReportPhotos.Add(originalPhoto);
            await _dbContext.SaveChangesAsync();

            var updatedPhoto = new ReportPhoto
            {
                Id = originalPhoto.Id,
                FilePath = "photos/new.jpg",
                FileType = "image/png",
                FileSize = 2048,
                DisasterReportId = 1
            };

            // Act
            var result = await _repository.UpdateAsync(updatedPhoto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("photos/new.jpg", result.FilePath);
            Assert.Equal("image/png", result.FileType);
            Assert.Equal(2048, result.FileSize);
        }

        [Fact]
        public async Task UpdateAsync_ShouldReturnNull_WhenPhotoNotFound()
        {
            // Arrange
            var nonExistingPhoto = new ReportPhoto { Id = 999 };

            // Act
            var result = await _repository.UpdateAsync(nonExistingPhoto);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByReportIdAsync_ShouldReturnPhotosForReport()
        {
                // Arrange
            var reportId = 1;
             _dbContext.ReportPhotos.AddRange(
                 new ReportPhoto
             {
                 DisasterReportId = reportId,
                FilePath = "photo1.jpg",
                FileType = "image/jpeg",
                 FileSize = 500
             },
             new ReportPhoto
             {
                 DisasterReportId = reportId,
                 FilePath = "photo2.jpg",
                 FileType = "image/png",
                 FileSize = 700
             },
             new ReportPhoto
             {
                 DisasterReportId = 2,
                 FilePath = "photo3.jpg",
                 FileType = "image/gif",
                 FileSize = 300
             }
           );

            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _repository.GetByReportIdAsync(reportId);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.All(result, p => Assert.Equal(reportId, p.DisasterReportId));
        }

      
        [Fact]
        public async Task GetByEventIdAsync_ShouldReturnPhotosForEvent()
        {
            // Arrange
            var eventId = 1;
            _dbContext.ReportPhotos.AddRange(
                new ReportPhoto
                {
                    DisasterEventId = eventId,
                    FilePath = "event1.jpg",
                    FileType = "image/jpeg",
                    FileSize = 1234
                },
                new ReportPhoto
                {
                    DisasterEventId = eventId,
                    FilePath = "event2.jpg",
                    FileType = "image/png",
                    FileSize = 5678
                },
                new ReportPhoto
                {
                    DisasterEventId = 2,
                    FilePath = "event3.jpg",
                    FileType = "image/gif",
                    FileSize = 2345
                }
            );
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _repository.GetByEventIdAsync(eventId);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.All(result, p => Assert.Equal(eventId, p.DisasterEventId));
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnPhoto_WhenExists()
        {
            // Arrange
            var photo = new ReportPhoto
            {
                FilePath = "test.jpg",
                FileType = "image/jpeg",
                FileSize = 1234
            };
            _dbContext.ReportPhotos.Add(photo);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _repository.GetByIdAsync(photo.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(photo.Id, result.Id);
        }
        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenNotExists()
        {
            // Act
            var result = await _repository.GetByIdAsync(999);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task DeleteAsync_ShouldRemovePhoto()
        {
            // Arrange
            var photo = new ReportPhoto
            {
                FilePath = "delete.jpg",
                FileType = "image/jpeg",
                FileSize = 512
            };
            _dbContext.ReportPhotos.Add(photo);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _repository.DeleteAsync(photo.Id);

            // Assert
            Assert.True(result);
            Assert.Empty(_dbContext.ReportPhotos);
        }

        [Fact]
        public async Task DeleteAsync_ShouldReturnFalse_WhenPhotoNotFound()
        {
            // Act
            var result = await _repository.DeleteAsync(999);

            // Assert
            Assert.False(result);
        }


    }
}

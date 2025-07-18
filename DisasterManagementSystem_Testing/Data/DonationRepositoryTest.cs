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
    public class DonationRepositoryTest
    {
        private AppDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb_DonationAppDb_" + Guid.NewGuid())
                .Options;

            var dbContext = new AppDbContext(options);
            return dbContext;
        }
        private readonly DonationRepository _repository;
        private readonly AppDbContext _context;

        public DonationRepositoryTest()
        {
            _context = GetInMemoryDbContext();
            _repository = new DonationRepository(_context);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnEmptyList_WhenNoDonationsExist()
        {
            // Act
            var result = await _repository.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsAllDonations()
        {
            // Arrange
            var testUser = new User
            {
                Id = Guid.NewGuid(),
                Name = "Test User",
                Email = "test@example.com",
                Role = "User", // Required field
                Status = "Active", // Required field
                PasswordHash = "hashedpassword", // If required
                AuthProvider = "Manual" // If required
            };

            _context.Users.Add(testUser);

            _context.Donations.AddRange(
                new Donation
                {
                    Id = 1,
                    Type = "Money",
                    Amount = 100,
                    Currency = "USD",
                    SourceType = "Personal",
                    DonorUserId = testUser.Id,
                    DonorUser = testUser,
                    Status = "Pending" // Required for Donation
                },
                new Donation
                {
                    Id = 2,
                    Type = "Item",
                    Name = "Blankets",
                    Quantity = 50,
                    Unit = "pieces",
                    SourceType = "Organization",
                    DonorUserId = testUser.Id,
                    DonorUser = testUser,
                    Status = "Pending" // Required for Donation
                });

            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Contains(result, d => d.Type == "Money");
            Assert.Contains(result, d => d.Type == "Item");
            Assert.All(result, d => Assert.NotNull(d.DonorUser));
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsDonation_WhenExists()
        {
            // Arrange
            var testUser = new User
            {
                Id = Guid.NewGuid(),
                Name = "Test User",
                Email = "test@example.com",
                Role = "Donor",                 // Required field
                Status = "Active",              // Required field
                PasswordHash = "hashed123",     // If required
                AuthProvider = "Manual",        // If required
                CreatedAt = DateTime.UtcNow     // If required
            };
            _context.Users.Add(testUser);

            var testDonation = new Donation
            {
                Id = 1,
                Type = "Money",
                Amount = 100,
                Currency = "USD",
                SourceType = "Personal",
                Status = "Pending",             // Required for Donation
               // CreatedAt = DateTime.UtcNow,     // If required
                DonorUserId = testUser.Id,
                DonorUser = testUser
            };
            _context.Donations.Add(testDonation);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("Money", result.Type);
            Assert.NotNull(result.DonorUser);
            Assert.Equal(testUser.Name, result.DonorUser.Name);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsNull_WhenNotExists()
        {
            // Act
            var result = await _repository.GetByIdAsync(999);

            // Assert
            Assert.Null(result);
        }
        [Fact]
        public async Task GetByUserIdAsync_ReturnsUserDonations()
        {
            // Arrange
            var userId = Guid.NewGuid();

            // Create a complete valid User with all required fields
            var testUser = new User
            {
                Id = userId,
                Name = "Test User",
                Email = "test@example.com",
                Role = "User",            // Required field
                Status = "Active",               // Required field
                PasswordHash = "hashedpassword", // If required
                AuthProvider = "Manual",         // If required
                CreatedAt = DateTime.UtcNow      // If required
            };

            _context.Users.Add(testUser);

            // Create test donations with all required fields
            _context.Donations.AddRange(
                new Donation
                {
                    Id = 1,
                    Type = "Money",
                    Amount = 100,
                    Currency = "USD",
                    SourceType = "Personal",
                    Status = "Pending",           // Required for Donation
                   
                    DonorUserId = userId,
                    DonorUser = testUser
                },
                new Donation
                {
                    Id = 2,
                    Type = "Item",
                    Name = "Blankets",
                    Quantity = 50,
                    Unit = "pieces",
                    SourceType = "Organization",
                    Status = "Pending",           // Required for Donation
                    
                    DonorUserId = userId,
                    DonorUser = testUser
                },
                new Donation
                {
                    Id = 3,
                    Type = "Money",
                    Amount = 200,
                    Currency = "USD",
                    SourceType = "Personal",
                    Status = "Pending",           // Required for Donation
                  
                    DonorUserId = Guid.NewGuid()   // Different user
                });

            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetByUserIdAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.All(result, d => Assert.Equal(userId, d.DonorUserId));
            Assert.All(result, d => Assert.NotNull(d.DonorUser));
            Assert.All(result, d => Assert.Equal("Test User", d.DonorUser.Name));
        }
        [Fact]
        public async Task CreateAsync_AddsNewDonation()
        {
            // Arrange
            var userId = Guid.NewGuid();

            // Create a complete valid User with all required fields
            var testUser = new User
            {
                Id = userId,
                Name = "Test User",
                Email = "test@example.com",
                Role = "Donor",                  // Required field
                Status = "Active",               // Required field
                PasswordHash = "hashedpassword", // If required
                AuthProvider = "Manual",         // If required
                CreatedAt = DateTime.UtcNow      // If required
            };
            _context.Users.Add(testUser);
            await _context.SaveChangesAsync();

            // Create a new donation with all required fields
            var newDonation = new Donation
            {
                Type = "Money",
                Amount = 100,
                Currency = "USD",
                SourceType = "Personal",
                Status = "Pending",              // Required for Donation
                
                DonorUserId = userId
            };

            // Act
            var result = await _repository.CreateAsync(newDonation);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Id > 0);
            Assert.Equal("Money", result.Type);

            // Verify the donation was actually saved to the database
            var dbDonation = await _context.Donations.FirstOrDefaultAsync();
            Assert.NotNull(dbDonation);
            Assert.Equal(result.Id, dbDonation.Id);
            Assert.Equal(userId, dbDonation.DonorUserId);
        }

        [Fact]
        public async Task UpdateAsync_ModifiesExistingDonation()
        {
            // Arrange
            var originalDonation = new Donation
            {
                Id = 1,
                Type = "Money",
                Amount = 100,
                SourceType = "Personal",
                Status = "Pending"
            };
            _context.Donations.Add(originalDonation);
            await _context.SaveChangesAsync();

            // Modify
            originalDonation.Amount = 150;
            originalDonation.Status = "Verified";

            // Act
            await _repository.UpdateAsync(originalDonation);

            // Assert
            var updatedDonation = await _context.Donations.FindAsync(1);
            Assert.NotNull(updatedDonation);
            Assert.Equal(150, updatedDonation.Amount);
            Assert.Equal("Verified", updatedDonation.Status);
        }
        [Fact]
        public async Task DeleteAsync_RemovesDonation()
        {
            // Arrange
            var userId = Guid.NewGuid();

            // Create a complete valid User with all required fields
            var testUser = new User
            {
                Id = userId,
                Name = "Test User",
                Email = "test@example.com",
                Role = "User",                  // Required field
                Status = "Active",               // Required field
                PasswordHash = "hashedpassword", // If required
                AuthProvider = "Manual",         // If required
                CreatedAt = DateTime.UtcNow      // If required
            };
            _context.Users.Add(testUser);

            // Create a complete valid Donation with all required fields
            var donation = new Donation
            {
                Id = 1,
                Type = "Money",
                Amount = 100,
                Currency = "USD",
                SourceType = "Personal",
                Status = "Pending",              // Required field
                DonorUserId = userId,
                DonorUser = testUser
            };

            _context.Donations.Add(donation);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.DeleteAsync(1);

            // Assert
            Assert.True(result);
            Assert.Empty(_context.Donations);
        }
        [Fact]
        public async Task DeleteAsync_ReturnsFalse_WhenNotExists()
        {
            // Act
            var result = await _repository.DeleteAsync(999);

            // Assert
            Assert.False(result);
        }

    }
}

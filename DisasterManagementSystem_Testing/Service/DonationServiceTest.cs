using DisasterManagementSystem_Data.Models;
using DisasterManagementSystem_Data.Repositories.Interfaces;
using DisasterManagementSystem_Services.Models;
using DisasterManagementSystem_Services.Services.Implements;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisasterManagementSystem_Testing.Service
{
    public class DonationServiceTest
    {
        private readonly DonationService _service;
        private readonly Mock<IDonationRepository> _donationRepoMock;
        private readonly Mock<IUserRepository> _userRepoMock;

        public DonationServiceTest()
        {
            _donationRepoMock = new Mock<IDonationRepository>();
            _userRepoMock = new Mock<IUserRepository>();
            _service = new DonationService(_donationRepoMock.Object, _userRepoMock.Object);
        }
        [Fact]
        public async Task CreateDonationAsync_ShouldReturnSuccess_ForValidMoneyDonation()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new User { Id = userId, Name = "Test User", Role = "Donor", Status = "Active" };
            var donationDto = new CreateDonationDto
            {
               
                Amount = 100,
                Currency = "USD",
                SourceType = "Personal"
            };

            _userRepoMock.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync(user);
            _donationRepoMock.Setup(x => x.CreateAsync(It.IsAny<Donation>()))
                .ReturnsAsync((Donation d) => d);

            // Act
            var result = await _service.CreateDonationAsync(donationDto, userId);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("Money", result.Data.Type);
            Assert.Equal(100, result.Data.Amount);
            Assert.Equal("Test User", result.Data.DonorName);
        }


        [Fact]
        public async Task CreateDonationAsync_ShouldReturnValidationError_ForInvalidMoneyDonation()
        {
            // Arrange
            var donationDto = new CreateDonationDto
            {
                //Type = "Money",
                Amount = -10, // Invalid amount
                SourceType = "Personal"
            };

            // Act
            var result = await _service.CreateDonationAsync(donationDto, Guid.NewGuid());

            // Assert
            Assert.False(result.IsSuccess);
            Assert.True(result.IsValidationError);
        }

        [Fact]
        public async Task GetAllDonationsAsync_ShouldReturnAllDonations()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var donations = new List<Donation>
            {
                new Donation { Id = 1,  DonorUserId = userId, DonorUser = new User { Name = "User1" } }
            };

            _donationRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(donations);

            // Act
            var result = await _service.GetAllDonationsAsync();

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(2, result.Data.Count);
        }
        [Fact]
        public async Task GetUserDonationsAsync_ShouldReturnOnlyUserDonations()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new User { Id = userId, Name = "Test User" };
            var donations = new List<Donation>
            {
                new Donation { Id = 1, DonorUserId = userId }
            };

            _userRepoMock.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync(user);
            _donationRepoMock.Setup(x => x.GetByUserIdAsync(userId)).ReturnsAsync(donations);

            // Act
            var result = await _service.GetUserDonationsAsync(userId);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(2, result.Data.Count);
            Assert.All(result.Data, d => Assert.Equal("Test User", d.DonorName));
        }

        [Fact]
        public async Task GetDonationByIdAsync_ShouldReturnDonation_WhenExists()
        {
            // Arrange
            var donation = new Donation
            {
                Id = 1,
              //  Type = "Money",
                DonorUserId = Guid.NewGuid(),
                DonorUser = new User { Name = "Test User" }
            };

            _donationRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(donation);

            // Act
            var result = await _service.GetDonationByIdAsync(1);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(1, result.Data.Id);
            Assert.Equal("Test User", result.Data.DonorName);
        }

        [Fact]
        public async Task GetDonationByIdAsync_ShouldReturnNotFound_WhenNotExists()
        {
            // Arrange
            _donationRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync((Donation)null);

            // Act
            var result = await _service.GetDonationByIdAsync(1);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.True(result.IsNotFoundError);
        }

    }
}

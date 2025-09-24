using DisasterManagementSystem_Api.Controllers;
using DisasterManagementSystem_Services.Models;
using DisasterManagementSystem_Services.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace DisasterManagementSystem_Testing.WebAPI
{
    public class DonationControllerTest
    {
        private readonly Mock<IDonationService> _mockService;
        private readonly DonationController _controller;

        public DonationControllerTest()
        {
            _mockService = new Mock<IDonationService>();
            _controller = new DonationController(_mockService.Object);

            // Mock user claims for authorized endpoints
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Role, "User")
            }));

            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user }
            };
        }
        [Fact]
        public async Task CreateDonation_ReturnsSuccess_ForValidRequest()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var donationDto = new CreateDonationDto
            {
                //Type = "Money",
                Amount = 100,
                Currency = "USD",
                SourceType = "Personal"
            };

            var expectedResult = Result<DonationDto>.Success(
                new DonationDto { Id = 1, Type = "Money", Amount = 100 },
                "Donation created successfully");

            _mockService.Setup(s => s.CreateDonationAsync(It.IsAny<CreateDonationDto>(), userId))
                       .ReturnsAsync(expectedResult);

            // Set user claim
            _controller.ControllerContext.HttpContext.User =
                new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.NameIdentifier, userId.ToString())
                }));

            // Act
            var result = await _controller.CreateDonation(donationDto);

            // Assert - For IResult return type
            var okResult = Assert.IsType<Ok<Result<DonationDto>>>(result);
            var returnedResult = okResult.Value;
            Assert.True(returnedResult.IsSuccess);
            Assert.Equal(1, returnedResult.Data.Id);
        }
        [Fact]
        public async Task GetMyDonations_ReturnsUserDonations()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var expectedDonations = new List<DonationDto>
            {
                new DonationDto { Id = 1, Type = "Money", DonorUserId = userId },
                new DonationDto { Id = 2, Type = "Item", DonorUserId = userId }
            };

            _mockService.Setup(s => s.GetUserDonationsAsync(userId))
                       .ReturnsAsync(Result<List<DonationDto>>.Success(expectedDonations));

            // Set user claim
            _controller.ControllerContext.HttpContext.User =
                new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.NameIdentifier, userId.ToString())
                }));

            // Act
            var result = await _controller.GetMyDonations();

            // Assert
            var okResult = Assert.IsType<Ok<Result<List<DonationDto>>>>(result);
            var returnedResult = okResult.Value;
            Assert.Equal(2, returnedResult.Data.Count);
        }

        [Fact]
        public async Task GetAllDonations_ReturnsAllDonations_ForAdmin()
        {
            // Arrange
            var expectedDonations = new List<DonationDto>
            {
                new DonationDto { Id = 1, Type = "Money" },
                new DonationDto { Id = 2, Type = "Item" }
            };

            _mockService.Setup(s => s.GetAllDonationsAsync())
                       .ReturnsAsync(Result<List<DonationDto>>.Success(expectedDonations));

            // Set admin claim
            _controller.ControllerContext.HttpContext.User =
                new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
                    new Claim(ClaimTypes.Role, "Admin")
                }));

            // Act
            var result = await _controller.GetAllDonations();

            // Assert
            var okResult = Assert.IsType<Ok<Result<List<DonationDto>>>>(result);
            var returnedResult = okResult.Value;
            Assert.Equal(2, returnedResult.Data.Count);
        }


        [Fact]
        public async Task GetDonationById_ReturnsDonation_WhenExists()
        {
            // Arrange
            var expectedDonation = new DonationDto { Id = 1, Type = "Money" };

            _mockService.Setup(s => s.GetDonationByIdAsync(1))
                       .ReturnsAsync(Result<DonationDto>.Success(expectedDonation));

            // Act
            var result = await _controller.GetDonationById(1);

            // Assert
            var okResult = Assert.IsType<Ok<Result<DonationDto>>>(result);
            var returnedResult = okResult.Value;
            Assert.Equal(1, returnedResult.Data.Id);
        }

        [Fact]
        public async Task GetAllDonations_ReturnsSuccess_ForAdmin()
        {
            // Arrange - User with Admin role
            var userId = Guid.NewGuid();
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Role, "Admin")
            }, "TestAuthentication"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            var expectedDonations = new List<DonationDto>
            {
                new DonationDto { Id = 1, Type = "Money" },
                new DonationDto { Id = 2, Type = "Item" }
            };

            _mockService.Setup(s => s.GetAllDonationsAsync())
                       .ReturnsAsync(Result<List<DonationDto>>.Success(expectedDonations));

            // Act
            var result = await _controller.GetAllDonations();

            // Assert
            var okResult = Assert.IsType<Ok<Result<List<DonationDto>>>>(result);
            Assert.Equal(2, okResult.Value.Data.Count);
        }


    }
}

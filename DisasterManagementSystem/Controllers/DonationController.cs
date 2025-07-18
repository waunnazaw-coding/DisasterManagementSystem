using DisasterManagementSystem_Services.Models;
using DisasterManagementSystem_Services.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DisasterManagementSystem_Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class DonationController : ControllerBase
    {
        private readonly IDonationService _donationService;

        public DonationController(IDonationService donationService)
        {
            _donationService = donationService;
        }

        [HttpPost]
        public async Task<IResult> CreateDonation([FromBody] CreateDonationDto donationDto)
        {
            if (!ModelState.IsValid)
                return Results.BadRequest(ModelState);

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out Guid userId))
                return Results.Unauthorized();

            var result = await _donationService.CreateDonationAsync(donationDto, userId);
            return result.Execute();
        }

        [HttpGet("my-donations")]
        public async Task<IResult> GetMyDonations()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out Guid userId))
                return Results.Unauthorized();

            var result = await _donationService.GetUserDonationsAsync(userId);
            return result.Execute();
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IResult> GetAllDonations()
        {
            var result = await _donationService.GetAllDonationsAsync();
            return result.Execute();
        }

        [HttpGet("{id}")]
        public async Task<IResult> GetDonationById(int id)
        {
            var result = await _donationService.GetDonationByIdAsync(id);
            return result.Execute();
        }

        //[Authorize(Roles = "Admin")]
        //[HttpPost("distribute")]
        ////public async Task<IResult> DistributeDonation([FromBody] DonationDistributionDto distributionDto)
        //{
        //    if (!ModelState.IsValid)
        //        return Results.BadRequest(ModelState);

        //    var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        //    if (!Guid.TryParse(userIdClaim, out Guid userId))
        //        return Results.Unauthorized();

        //    var result = await _donationService.DistributeDonationAsync(distributionDto, userId);
        //    return result.Execute();
        //}
    }
}

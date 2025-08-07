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

        // New endpoint for updating donation status
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}/status")]
        public async Task<IResult> UpdateDonationStatus(int id, [FromBody] UpdateStatusDto statusDto)
        {
            if (!ModelState.IsValid)
                return Results.BadRequest(ModelState);

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out Guid userId))
                return Results.Unauthorized();

            var result = await _donationService.UpdateDonationStatusAsync(id, statusDto.Status, userId);
            return result.Execute();
        }

        // New endpoint for updating donation by user
        [HttpPut("{id}")]
        public async Task<IResult> UpdateDonation(int id, [FromBody] UpdateDonationDto donationDto)
        {
            if (!ModelState.IsValid)
                return Results.BadRequest(ModelState);

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out Guid userId))
                return Results.Unauthorized();

            var result = await _donationService.UpdateDonationAsync(id, donationDto, userId);
            return result.Execute();
        }

        // New endpoint for deleting donation by user
        [HttpDelete("{id}")]
        public async Task<IResult> DeleteDonation(int id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out Guid userId))
                return Results.Unauthorized();

            var result = await _donationService.DeleteDonationAsync(id, userId);
            return result.Execute();
        }
        [HttpGet("recent")]
        public async Task<IResult> GetRecentDonations()
       {
            var result = await _donationService.GetRecentDonationsAsync();
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

using DisasterManagementSystem_Services.Models.PartnerDtos;
using DisasterManagementSystem_Services.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DisasterManagementSystem_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PartnersController : ControllerBase
    {

        private readonly IPartnerService _partnerService;

        

            public PartnersController(IPartnerService partnerService)
            {
                _partnerService = partnerService;
            }

            [HttpPost]
            public async Task<IResult> CreatePartner([FromForm] PartnerCreateDTO partnerDto)
            {
                if (!ModelState.IsValid)
                    return Results.BadRequest(ModelState);

                var result = await _partnerService.CreatePartnerAsync(partnerDto);
                return result.Execute();
            }

            [HttpGet("{id:int}")]
            [AllowAnonymous]
            public async Task<IResult> GetPartner(int id)
            {
                var result = await _partnerService.GetPartnerAsync(id);
                return result.Execute();
            }

            [HttpGet]
            [AllowAnonymous]
            public async Task<IResult> GetAllPartners()
            {
                var result = await _partnerService.GetAllPartnersAsync();
                return result.Execute();
            }

            [HttpPut]
            public async Task<IResult> UpdatePartner([FromForm] PartnerUpdateDTO partnerDto)
            {
                if (!ModelState.IsValid)
                    return Results.BadRequest(ModelState);

                var result = await _partnerService.UpdatePartnerAsync(partnerDto);
                return result.Execute();
            }

            [HttpDelete("{id:int}")]
            public async Task<IResult> DeletePartner(int id)
            {
                var result = await _partnerService.DeletePartnerAsync(id);
                return result.Execute();
            }

        [HttpPatch("{id:int}/status")]
        public async Task<IResult> UpdatePartnerStatus(int id, [FromBody] PartnerStatusUpdateDto statusDto)
        {
            if (string.IsNullOrEmpty(statusDto?.Status))
            {
                return Results.BadRequest("Status is required");
            }

            var result = await _partnerService.UpdatePartnerStatusAsync(id, statusDto.Status);
            return result.Execute();
        }

        [HttpGet("public")]
        [AllowAnonymous]
        public async Task<IResult> GetPublicPartners()
        {
            var result = await _partnerService.GetPublicPartnersAsync();
            return result.Execute();
        }
    }

    // Add this DTO class to your PartnerDtos namespace
    public class PartnerStatusUpdateDto
    {
        public string Status { get; set; }
    }


}
    

using DisasterManagementSystem_Services.Models;
using DisasterManagementSystem_Services.Services.Interfaces;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace DisasterManagementSystem_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportPhotoController : ControllerBase
    {
        private readonly IReportPhotoService _photoService;

        public ReportPhotoController(IReportPhotoService photoService)
        {
            _photoService = photoService;
        }

        /// <summary>
        /// Upload photos for a Disaster Report
        /// </summary>
        /// <param name="reportId">The ID of the disaster report</param>
        /// <param name="files">Image files to upload</param>
        [HttpPost("upload/report")]
        public async Task<IResult> UploadReportPhotos([FromQuery] int reportId, [FromForm] IFormFile[] files)
        {
            if (files == null || files.Length == 0)
                return Results.BadRequest("Please select at least one file.");

            var result = await _photoService.UploadReportPhotosAsync(reportId, files);
            return result.Execute();
        }

        /// <summary>
        /// Upload photos for a Disaster Event
        /// </summary>
        /// <param name="eventId">The ID of the disaster event</param>
        /// <param name="files">Image files to upload</param>
        [HttpPost("upload/event")]
        public async Task<IResult> UploadEventPhotos([FromQuery] int eventId, [FromForm] IFormFile[] files)
        {
            if (files == null || files.Length == 0)
                return Results.BadRequest("Please select at least one file.");

            var result = await _photoService.UploadEventPhotosAsync(eventId, files);
            return result.Execute();
        }

        /// <summary>
        /// Update a specific photo
        /// </summary>
        /// <param name="photoId">The ID of the photo to update</param>
        [HttpPut("{photoId}")]
        public async Task<IResult> UpdatePhoto(int photoId, [FromForm] UpdatePhotoDto dto)
        {
            if (dto.File == null || dto.File.Length == 0)
                return Results.BadRequest("Please select a file to upload.");

            var result = await _photoService.UpdatePhotoAsync(photoId, dto.File);
            return result.Execute();
        }


        /// <summary>
        /// Get all photos for a specific disaster report
        /// </summary>
        /// <param name="reportId">The ID of the disaster report</param>
        [HttpGet("report/{reportId}")]
        public async Task<IResult> GetPhotosByReportId(int reportId)
        {
            var result = await _photoService.GetPhotosByReportIdAsync(reportId);
            return result.Execute();
        }

        /// <summary>
        /// Get all photos for a specific disaster event
        /// </summary>
        /// <param name="eventId">The ID of the disaster event</param>
        [HttpGet("event/{eventId}")]
        public async Task<IResult> GetPhotosByEventId(int eventId)
        {
            var result = await _photoService.GetPhotosByEventIdAsync(eventId);
            return result.Execute();
        }

        /// <summary>
        /// Get a specific photo by ID
        /// </summary>
        /// <param name="photoId">The ID of the photo</param>
        [HttpGet("{photoId}")]
        public async Task<IResult> GetPhotoById(int photoId)
        {
            var result = await _photoService.GetPhotoByIdAsync(photoId);
            return result.Execute();
        }

        /// <summary>
        /// Delete a specific photo
        /// </summary>
        /// <param name="photoId">The ID of the photo to delete</param>
        [HttpDelete("{photoId}")]
        public async Task<IResult> DeletePhoto(int photoId)
        {
            var result = await _photoService.DeletePhotoAsync(photoId);
            return result.Execute();
        }
    }
}
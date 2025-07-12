using DisasterManagementSystem_Services.Services.Interfaces;
using Microsoft.AspNetCore.Http;
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
        /// Upload photos for a Disaster Report (standalone)
        /// POST: /api/ReportPhoto/upload-report
        /// </summary>
        [HttpPost("upload-report")]
        public async Task<IResult> UploadReportPhotos([FromQuery] int reportId, [FromForm] IFormFile[] files)
        {
            if (files == null || files.Length == 0)
                return Results.BadRequest("Please select at least one file.");

            var result = await _photoService.UploadReportPhotosAsync(reportId, files);
            return result.Execute();
        }

        /// <summary>
        /// Upload photos for a Disaster Event (standalone)
        /// POST: /api/ReportPhoto/upload-event
        /// </summary>
        [HttpPost("upload-event")]
        public async Task<IResult> UploadEventPhotos([FromQuery] int eventId, [FromForm] IFormFile[] files)
        {
            if (files == null || files.Length == 0)
                return Results.BadRequest("Please select at least one file.");

            var result = await _photoService.UploadEventPhotosAsync(eventId, files);
            return result.Execute();
        }
    }
}

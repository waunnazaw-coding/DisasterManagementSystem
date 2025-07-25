using DisasterManagementSystem_Services.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace DisasterManagementSystem_Services.Services
{
    public interface IReportPhotoService
    {
        Task<Result<List<UploadPhotoResultDTO>>> UploadReportPhotosAsync(int disasterReportId, IFormFile[] files);
        Task<Result<List<UploadPhotoResultDTO>>> UploadEventPhotosAsync(int disasterEventId, IFormFile[] files);
        Task<Result<UploadPhotoResultDTO>> UpdatePhotoAsync(int photoId, IFormFile file);
        Task<Result<List<UploadPhotoResultDTO>>> GetPhotosByReportIdAsync(int reportId);
        Task<Result<List<UploadPhotoResultDTO>>> GetPhotosByEventIdAsync(int eventId);
        Task<Result<UploadPhotoResultDTO>> GetPhotoByIdAsync(int photoId);
        Task<Result<bool>> DeletePhotoAsync(int photoId);
    }
}

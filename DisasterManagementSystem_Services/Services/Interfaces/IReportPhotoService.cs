using DisasterManagementSystem_Services.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisasterManagementSystem_Services.Services.Interfaces
{
    public interface IReportPhotoService
    {
        Task <Result<List<UploadPhotoResultDTO>>> UploadReportPhotosAsync(int disasterReportId, IFormFile[] files);
        Task <Result<List<UploadPhotoResultDTO>>> UploadEventPhotosAsync(int disasterEventId, IFormFile[] files);
    }
}

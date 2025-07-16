using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisasterManagementSystem_Services.Models
{
    public class UpdatePhotoDto
    {
        public IFormFile File { get; set; } = null!;
        public string? ContentType { get; set; }
        public string? ContentDisposition { get; set; }
        public string? Name { get; set; }
        public string? FileName { get; set; }
        public long? Length { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisasterManagementSystem_Services.Models
{
    public class UploadPhotoResultDTO
    {
        public int Id { get; set; }
        public string? Description { get; set; }
        public string FilePath { get; set; } = null!;
        public string FileType { get; set; } = null!;
        public long? FileSize { get; set; }
        public DateTime? UploadedAt { get; set; }
        public bool IsVideo { get; set; }  // New computed property
    }
}

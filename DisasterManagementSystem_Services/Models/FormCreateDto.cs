using System;

namespace DisasterManagementSystem_Services.Models
{
    public class FormCreateDto
    {
        // ---------- Location ----------
        public string LocationName { get; set; } = null!;
        public string GeoJson { get; set; } = null!;
        public string? Address { get; set; }
        public string? Country { get; set; }
        public string? Region { get; set; }

        // ---------- DisasterReport ----------
        public Guid UserId { get; set; }                  // required
        public int? DisasterEventId { get; set; }         // optional
        public string? AddressDetail { get; set; }
        public string Type { get; set; } = null!;         // required
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Severity { get; set; }
        public string? Source { get; set; }

        // ---------- DisasterType ----------
        public string DisasterTypeName { get; set; } = null!;
        public string Category { get; set; } = null!;
        public string? DisasterTypeDescription { get; set; }

        // ---------- ReportPhoto ----------
        public string FilePath { get; set; } = null!;
        public string FileType { get; set; } = null!;
        public long? FileSize { get; set; }
        public DateTime? UploadedAt { get; set; }
    }
}

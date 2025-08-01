using System;
using Microsoft.AspNetCore.Http;

namespace DisasterManagementSystem_Services.Models
{
    public class FormUpdateDto
    {
        public int Id { get; set; }  // Required for update

        // ---------- Location ----------
        public string? LocationName { get; set; }
        public string? GeoJson { get; set; }

        // ---------- DisasterReport ----------
        public Guid? UserId { get; set; }  // optional: depends if you want to allow changes
        public int? DisasterEventId { get; set; }
        public string? AddressDetail { get; set; }
        public string? Type { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Severity { get; set; }
        public string? Source { get; set; }

        // ---------- ReportPhoto ----------
        public IFormFile[]? Files { get; set; }
        public List<string> NewPhotoDescriptions { get; set; }
    }
}

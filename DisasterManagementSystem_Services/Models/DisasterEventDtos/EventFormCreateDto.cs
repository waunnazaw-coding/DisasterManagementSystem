using System;
using Microsoft.AspNetCore.Http;

namespace DisasterManagementSystem_Services.Models
{
    public class EventFormCreateDto
    {
        // ---------- Location ----------
        public string LocationName { get; set; } = null!;
        public string GeoJson { get; set; } = null!;

        // ---------- DisasterEvent ----------
        public string Name { get; set; } = null!;
        public int DisasterTypeId { get; set; }
        public DateOnly StartDate { get; set; }
        public int LocationId { get; set; }  // Will be set server-side after location is created
        public string? Severity { get; set; }
        public string? Description { get; set; }
        public List<string> NewPhotoDescription { get; set; } = new List<string>();

        // ---------- ReportPhoto ----------
        public IFormFile[] ReportPhotos { get; set; } = Array.Empty<IFormFile>();
    }
}

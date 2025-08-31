using System;
using Microsoft.AspNetCore.Http;

namespace DisasterManagementSystem_Services.Models
{
    public class FormCreateDto
    {
        // ---------- Location ----------
        public int LocationId { get; set; }
        public string LocationName { get; set; } = null!;
        public string GeoJson { get; set; } = null!;

        // ---------- DisasterReport ----------
        public Guid? UserId { get; set; }                  // required
        public int? DisasterEventId { get; set; }         // optional
        public string? AddressDetail { get; set; }
        public string Type { get; set; } = null!;         // required
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Severity { get; set; }
        public string? Source { get; set; }
        public DateOnly StartDate { get; set; }
        public string? Status { get; set; }

        // ---------- ReportPhoto ----------    
        public IFormFile[] ReportPhotos { get; set; } = Array.Empty<IFormFile>();
        public List<string> NewPhotoDescription { get; set; }
    }
}

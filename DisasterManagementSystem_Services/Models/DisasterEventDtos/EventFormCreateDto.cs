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
        public int? Id { get; set; }
        public string Name { get; set; } = null!;
        public int DisasterTypeId { get; set; }
        public DateOnly StartDate { get; set; }
        public int LocationId { get; set; }  // Will be set server-side after location is created
        public string? Severity { get; set; }
        public string? Description { get; set; }
        public string? Source { get; set; }

        // User info
        public Guid CreatedUserId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public Guid? UpdatedUserId { get; set; }       // null on create
        public DateTime? UpdatedAt { get; set; }       // null on create

        // ---------- ReportPhoto ----------
        public IFormFile[] ReportPhotos { get; set; } = Array.Empty<IFormFile>();
        public List<string> NewPhotoDescription { get; set; } = new();

    }
}

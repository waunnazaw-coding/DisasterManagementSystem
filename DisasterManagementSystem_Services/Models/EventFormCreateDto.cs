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
        public DateOnly? EndDate { get; set; }
        public int LocationId { get; set; }
        public string? Severity { get; set; }
        public string Status { get; set; } = null!;
        public string? Description { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // ---------- ReportPhoto ----------
        public IFormFile[] Files { get; set; } = Array.Empty<IFormFile>();
    }
}

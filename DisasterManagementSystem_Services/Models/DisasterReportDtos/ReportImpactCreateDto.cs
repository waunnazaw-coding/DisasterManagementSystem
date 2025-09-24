using System;
using Microsoft.AspNetCore.Http;

namespace DisasterManagementSystem_Services.Models
{
    public class ReportImpactCreateDto
    {
        public string LocationName { get; set; } = null!;
        public string? Address { get; set; }
        public string? Region { get; set; }
        public string? Country { get; set; }
        public string GeoJson { get; set; } = null!;
        public Guid UserId { get; set; }                 
 
        public string? AddressDetail { get; set; }
        public string Type { get; set; } = null!;      
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Severity { get; set; }
        public string? Source { get; set; }
        public string? Status { get; set; }
        public DateOnly StartDate { get; set; }

        public IFormFile[] ReportPhotos { get; set; } = Array.Empty<IFormFile>();
        public List<string> NewPhotoDescription { get; set; }
        //public List<ImpactCreateDto> Impacts { get; set; } = new();

        public string ImpactsJson { get; set; } = string.Empty;
    }

    public class ImpactCreateDto
    {
        public string Type { get; set; } = null!;
        public string? Value { get; set; }
        public string? ObjectName { get; set; }
    }
}

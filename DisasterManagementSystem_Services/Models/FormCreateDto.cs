namespace DisasterManagementSystem_Services.Models
{
    public class FormCreateDto
    {
        // LocationDtos
        public string Name { get; set; } = null!;
        public string GeoJson { get; set; } = null!;

        //  DisasterReportDtos
        public string? AddressDetail { get; set; }
        public string Type { get; set; } = null!;
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Severity { get; set; }
        public string? Source { get; set; }

        // DisasterTypeDtos
        public string DisasterTypeName { get; set; } = null!;
        public string Category { get; set; } = null!;
        public string? DisasterTypeDescription { get; set; }

        // ReportPhotoDtos
        public string FilePath { get; set; } = null!;
        public string FileType { get; set; } = null!;
        public long? FileSize { get; set; }
        public DateTime? UploadedAt { get; set; }
    }
}

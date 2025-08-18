namespace DisasterManagementSystem_Services.Models
{
    public class DisasterEventListDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string DisasterTypeName { get; set; } = null!;

        // Changed StartDate from DateOnly to DateTime
        public DateOnly StartDate { get; set; }

        // Location
        public string LocationName { get; set; } = null!;
        public string? Region { get; set; }
        public string? Country { get; set; }
        public string? Address { get; set; }
        public string LocationGeoJson { get; set; }

        // Event Details
        public string? Severity { get; set; }
        public string Status { get; set; } = null!;
        public string? Description { get; set; }
        public string? Source { get; set; }

        // Auditing
        public Guid CreatedUserId { get; set; }
        public string? CreatedUserName { get; set; }
        public DateTime? CreatedAt { get; set; }
        public Guid? UpdatedUserId { get; set; }
        public string? UpdatedUserName { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // New computed property for total affected people
        public int? AffectedPeople { get; set; }
        public int AffectedFamilies { get; set; }
        public int AffectedInfrastructures { get; set; }
        public List<string> CurrencyChanges { get; set; }
        public string? FirstImageUrl { get; set; }
    }
}

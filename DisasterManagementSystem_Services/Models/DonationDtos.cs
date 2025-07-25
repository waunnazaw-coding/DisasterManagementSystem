using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisasterManagementSystem_Services.Models
{
    public class CreateDonationDto
    {
        [Required(ErrorMessage = "Donation type is required")]
        public string Type { get; set; } // "Money" or "Item"

        public string? Name { get; set; } // For item donations
        public string? Description { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
        public decimal? Quantity { get; set; } // For item donations
        public string? Unit { get; set; } // For item donations

        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal? Amount { get; set; } // For money donations
        public string? Currency { get; set; } // For money donations

        [Required(ErrorMessage = "Source type is required")]
        public string SourceType { get; set; } // "Personal", "Organization", etc.
    }

    public class DonationDto
    {
        public int Id { get; set; }
        public Guid? DonorUserId { get; set; }
        public string? DonorName { get; set; }
        public string Type { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public decimal? Quantity { get; set; }
        public string? Unit { get; set; }
        public decimal? Amount { get; set; }
        public string? Currency { get; set; }
        public DateTime DateReceived { get; set; }
        public string SourceType { get; set; }
        public string Status { get; set; }
    }

    public class DonationDistributionDto
    {
        public int DonationId { get; set; }
        public int? AssistanceRequestId { get; set; }
        public int? BeneficiaryReliefTeamId { get; set; }
        public decimal? Quantity { get; set; }
        public decimal? Amount { get; set; }
        public string? DistributionNotes { get; set; }
    }
}

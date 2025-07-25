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
        public string Type { get; set; } = null!; // "Money" or "Item"

        [StringLength(200, ErrorMessage = "Name cannot exceed 200 characters")]
        public string? Name { get; set; }

        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        public string? Description { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
        public decimal? Quantity { get; set; } // For item donations

        [StringLength(50, ErrorMessage = "Unit cannot exceed 50 characters")]
        public string? Unit { get; set; } // For item donations

        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal? Amount { get; set; } // For money donations

        [StringLength(10, ErrorMessage = "Currency cannot exceed 10 characters")]
        public string? Currency { get; set; } // For money donations

        [StringLength(50, ErrorMessage = "Payment method cannot exceed 50 characters")]
        public string? PaymentMethod { get; set; } // "KPay", "WavePay", "BankTransfer"

        [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
        [RegularExpression(@"^[\+]?[0-9\-$$$$\s]+$", ErrorMessage = "Invalid phone number format")]
        public string? DonorPhoneNumber { get; set; } // Optional donor phone number

        [Required(ErrorMessage = "Source type is required")]
        [StringLength(50, ErrorMessage = "Source type cannot exceed 50 characters")]
        public string SourceType { get; set; } = null!; // "Personal", "Organization", etc.
    }

    public class DonationDto
    {
        public int Id { get; set; }
        public Guid? DonorUserId { get; set; }
        public string? DonorName { get; set; }
        public string Type { get; set; } = null!;
        public string? Name { get; set; }
        public string? Description { get; set; }
        public decimal? Quantity { get; set; }
        public string? Unit { get; set; }
        public decimal? Amount { get; set; }
        public string? Currency { get; set; }
        public string? PaymentMethod { get; set; }
        public string? DonorPhoneNumber { get; set; }
        public DateTime? DateReceived { get; set; }
        public string SourceType { get; set; } = null!;
        public string Status { get; set; } = null!;
    }

    public class UpdateStatusDto
    {
        [Required(ErrorMessage = "Status is required")]
        [StringLength(50, ErrorMessage = "Status cannot exceed 50 characters")]
        public string Status { get; set; } = null!;
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
    public class UpdateDonationDto
    {
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Type { get; set; } = string.Empty; // "Money" or "Item"

        [StringLength(1000)]
        public string? Description { get; set; }

        // For Money donations
        public decimal? Amount { get; set; }
        public string? Currency { get; set; }
        public string? PaymentMethod { get; set; }

        // For Item donations
        public int? Quantity { get; set; }
        public string? Unit { get; set; }

        [Required]
        public string SourceType { get; set; } = string.Empty;

        [Phone]
        public string? DonorPhoneNumber { get; set; }
    }


}

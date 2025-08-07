using System;
using System.Collections.Generic;

namespace DisasterManagementSystem_Data.Models;

public partial class Donation
{
    public int Id { get; set; }

    public Guid? DonorUserId { get; set; }

    public string? Name { get; set; }

    public string Type { get; set; } = null!;

    public string? Description { get; set; }

    public decimal? Quantity { get; set; }

    public string? Unit { get; set; }

    public decimal? Amount { get; set; }

    public string? Currency { get; set; }

    public DateTime? DateReceived { get; set; }

    public string SourceType { get; set; } = null!;

    public string Status { get; set; } = null!;

    public string? PaymentMethod { get; set; }

    public string? DonorPhoneNumber { get; set; }

    public virtual ICollection<DonationDistribution> DonationDistributions { get; set; } = new List<DonationDistribution>();

    public virtual User? DonorUser { get; set; }
}

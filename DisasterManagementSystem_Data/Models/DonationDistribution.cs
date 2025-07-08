using System;
using System.Collections.Generic;

namespace DisasterManagementSystem_Data.Models;

public partial class DonationDistribution
{
    public int Id { get; set; }

    public int DonationId { get; set; }

    public int? AssistanceRequestId { get; set; }

    public int? BeneficiaryReliefTeamId { get; set; }

    public decimal? Quantity { get; set; }

    public decimal? Amount { get; set; }

    public DateTime? DateDistributed { get; set; }

    public string Status { get; set; } = null!;

    public Guid? DistributedBy { get; set; }

    public string? DistributionNotes { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual AssistanceRequest? AssistanceRequest { get; set; }

    public virtual ReliefTeam? BeneficiaryReliefTeam { get; set; }

    public virtual User? DistributedByNavigation { get; set; }

    public virtual Donation Donation { get; set; } = null!;
}

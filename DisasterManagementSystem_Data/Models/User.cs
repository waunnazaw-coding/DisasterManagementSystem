using System;
using System.Collections.Generic;

namespace DisasterManagementSystem_Data.Models;

public partial class User
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? PasswordHash { get; set; }

    public string? AuthProvider { get; set; }

    public string? ExternalId { get; set; }

    public string Role { get; set; } = null!;

    public string Status { get; set; } = null!;
    
    public string? RefreshToken { get; set; }
    
    public DateTime? RefreshTokenExpiryTime { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<AssistanceRequest> AssistanceRequests { get; set; } = new List<AssistanceRequest>();

    public virtual ICollection<DisasterReport> DisasterReports { get; set; } = new List<DisasterReport>();

    public virtual ICollection<DonationDistribution> DonationDistributions { get; set; } = new List<DonationDistribution>();

    public virtual ICollection<Donation> Donations { get; set; } = new List<Donation>();

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual ICollection<ReliefTeamActivity> ReliefTeamActivities { get; set; } = new List<ReliefTeamActivity>();

    public virtual ICollection<RequestAssignment> RequestAssignmentAssignedByNavigations { get; set; } = new List<RequestAssignment>();

    public virtual ICollection<RequestAssignment> RequestAssignmentLastUpdatedByNavigations { get; set; } = new List<RequestAssignment>();
}

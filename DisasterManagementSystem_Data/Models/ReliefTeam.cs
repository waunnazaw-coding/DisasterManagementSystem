using System;
using System.Collections.Generic;

namespace DisasterManagementSystem_Data.Models;

public partial class ReliefTeam
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? ContactInfo { get; set; }

    public int? LocationId { get; set; }

    public string? Address { get; set; }

    public string Status { get; set; } = null!;

    public string? TeamLeaderName { get; set; }

    public string? SocialMediaUrl { get; set; }

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public int? NumberOfMembers { get; set; }

    public string? Specialization { get; set; }

    public string? EquipmentDetails { get; set; }

    public DateOnly? EstablishedDate { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public Guid? UserId { get; set; }

    public virtual ICollection<DonationDistribution> DonationDistributions { get; set; } = new List<DonationDistribution>();

    public virtual Location? Location { get; set; }

    public virtual ICollection<ReliefTeamActivity> ReliefTeamActivities { get; set; } = new List<ReliefTeamActivity>();

    public virtual ICollection<RequestAssignment> RequestAssignments { get; set; } = new List<RequestAssignment>();
    
    public virtual ICollection<UserReliefTeam> UserReliefTeams { get; set; } = new List<UserReliefTeam>();
}

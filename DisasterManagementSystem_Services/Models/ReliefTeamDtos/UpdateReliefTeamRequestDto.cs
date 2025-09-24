using System.ComponentModel.DataAnnotations;

namespace DisasterManagementSystem_Services.Models.ReliefTeamDtos;

public class UpdateReliefTeamRequestDto
{
    public string Name { get; set; } = default!;

    public string Email { get; set; } = default!;

    public string ContactInfo { get; set; } = default!;

    public int LocationId { get; set; }

    public string Address { get; set; } = default!;

    public string? Status { get; set; }

    public string? TeamLeaderName { get; set; }

    public string? SocialMediaURL { get; set; }

    public string? Phone { get; set; }

    public int? NumberOfMembers { get; set; }

    public string? Specialization { get; set; }


    public string? EquipmentDetails { get; set; }

    public DateOnly? EstablishedDate { get; set; }
}
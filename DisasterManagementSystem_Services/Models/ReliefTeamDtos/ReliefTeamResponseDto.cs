namespace DisasterManagementSystem_Services.Models.ReliefTeamDtos;

public class ReliefTeamResponseDTO
{
    public int Id { get; set; }                          
    public string Name { get; set; }
    public string ContactInfo { get; set; }
    public int? LocationId { get; set; }
    public string Address { get; set; }
    public string Status { get; set; }
    public string TeamLeaderName { get; set; }
    public string SocialMediaURL { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
    public int? NumberOfMembers { get; set; }
    public string Specialization { get; set; }
    public string EquipmentDetails { get; set; }
    public DateOnly? EstablishedDate { get; set; }
    public DateTime? CreatedAt { get; set; }          
    public DateTime? UpdatedAt { get; set; }          
}

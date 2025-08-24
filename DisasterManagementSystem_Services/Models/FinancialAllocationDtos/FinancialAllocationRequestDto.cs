namespace DisasterManagementSystem_Services.Models.FinancialAllocationDtos;

public class FinancialAllocationRequestDto
{
    //public int? DonationId { get; set; }
    public string AllocationTypeName { get; set; } = null!;
    public decimal Amount { get; set; }
    public DateTime AllocationDate { get; set; }
    //public Guid? CreatedBy { get; set; }
    public string? Notes { get; set; }
    public string DetailName { get; set; } = null!;
    public string? DetailDescription { get; set; }
}
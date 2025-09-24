using System;
using System.Collections.Generic;

namespace DisasterManagementSystem_Data.Models;

public partial class FinancialAllocation
{
    public int Id { get; set; }

    public int? DonationId { get; set; }

    public int AllocationTypeId { get; set; }

    public decimal Amount { get; set; }

    public DateTime AllocationDate { get; set; }

    public Guid? CreatedBy { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string? Notes { get; set; }

    public string DetailName { get; set; } = null!;

    public string? DetailDescription { get; set; }

    public virtual AllocationType AllocationType { get; set; } = null!;

    public virtual Donation? Donation { get; set; }
}

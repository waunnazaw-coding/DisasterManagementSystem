using System;
using System.Collections.Generic;

namespace DisasterManagementSystem_Data.Models;

public partial class AllocationType
{
    public int AllocationTypeId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public virtual ICollection<FinancialAllocation> FinancialAllocations { get; set; } = new List<FinancialAllocation>();
}

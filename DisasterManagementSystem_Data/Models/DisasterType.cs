using System;
using System.Collections.Generic;

namespace DisasterManagementSystem_Data.Models;

public partial class DisasterType
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Category { get; set; } = null!;

    public string? Description { get; set; }

    public virtual ICollection<DisasterEvent> DisasterEvents { get; set; } = new List<DisasterEvent>();
}

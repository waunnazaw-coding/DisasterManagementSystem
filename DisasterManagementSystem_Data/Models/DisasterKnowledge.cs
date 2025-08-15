using System;
using System.Collections.Generic;

namespace DisasterManagementSystem_Data.Models;

public partial class DisasterKnowledge
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public string ContentType { get; set; } = null!;

    public string? DisasterType { get; set; }

    public int? AuthorId { get; set; }

    public string? Content { get; set; }

    public string Language { get; set; } = null!;

    public string? ReferenceFrom { get; set; }

    public DateTime DateCreated { get; set; }

    public DateTime DateUpdated { get; set; }

    public virtual ICollection<Resource> Resources { get; set; } = new List<Resource>();
}

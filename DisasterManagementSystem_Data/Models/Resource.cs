using System;
using System.Collections.Generic;

namespace DisasterManagementSystem_Data.Models;

public partial class Resource
{
    public int Id { get; set; }

    public int DisasterKnowledgeId { get; set; }

    public string ResourceType { get; set; } = null!;

    public string Url { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime DateAdded { get; set; }

    public long? FileSize { get; set; }

    public virtual DisasterKnowledge DisasterKnowledge { get; set; } = null!;
}

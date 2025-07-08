using System;
using System.Collections.Generic;

namespace DisasterManagementSystem_Data.Models;

public partial class ReliefTeamActivity
{
    public int Id { get; set; }

    public int? ReliefTeamId { get; set; }

    public Guid? UserId { get; set; }

    public DateTime ActivityDate { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public string? DetailedAddress { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ReliefTeam? ReliefTeam { get; set; }

    public virtual User? User { get; set; }
}

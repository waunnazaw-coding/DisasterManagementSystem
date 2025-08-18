using System;
using System.Collections.Generic;

namespace DisasterManagementSystem_Data.Models;

public partial class ReliefTeamActivity
{
    public int Id { get; set; }

    public int ReliefTeamId { get; set; }

    public Guid PostedBy { get; set; }

    public DateTime ActivityDate { get; set; }

    public string Title { get; set; } = null!;

    public string Description { get; set; } = null!;

    public string? DetailedAddress { get; set; }

    public DateTime CreatedAt { get; set; }

    public string ActivityType { get; set; } = null!;

    public int? PeopleHelped { get; set; }

    public string? ItemsDistributed { get; set; }

    public decimal? ExpenseAmount { get; set; }

    public virtual User PostedByNavigation { get; set; } = null!;

    public virtual ReliefTeam ReliefTeam { get; set; } = null!;

    public virtual ICollection<ReportPhoto> ReportPhotos { get; set; } = new List<ReportPhoto>();
}

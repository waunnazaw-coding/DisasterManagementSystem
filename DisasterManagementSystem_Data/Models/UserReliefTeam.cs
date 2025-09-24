using System;
using System.Collections.Generic;

namespace DisasterManagementSystem_Data.Models;

public partial class UserReliefTeam
{
    public int Id { get; set; }

    public Guid UserId { get; set; }

    public int ReliefTeamId { get; set; }

    public virtual ReliefTeam ReliefTeam { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}

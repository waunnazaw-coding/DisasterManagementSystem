using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisasterManagementSystem_Services.Models.ReliefTeamActivityDtos
{
    public class ActivityStatsDTO
    {
        public int TotalActivities { get; set; }
        public Dictionary<string, int> ActivitiesByType { get; set; } = new();
        public List<ReliefTeamActivityDTO> RecentActivities { get; set; } = new();
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisasterManagementSystem_Services.Models.AssistanceRequestDtos
{
    public class RequestStatsDto
    {
        public int TotalCount { get; set; }
        public int PendingCount { get; set; }
        public int ApprovedCount { get; set; }
        //public int InProgressCount { get; set; }
        public int FulfilledCount { get; set; }
        public int RejectedCount { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisasterManagementSystem_Services.Models.ReliefTeamActivityDtos
{
    public class ReliefTeamActivityDTO
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
        public string ReliefTeamName { get; set; } 
        public string PostedByUserName { get; set; } 
        public List<ActivityMediaDTO> Media { get; set; } = new List<ActivityMediaDTO>();
    }

    public class ActivityMediaDTO
    {
        public int Id { get; set; }
        public string FilePath { get; set; } = null!;
        public string FileType { get; set; } = null!;
        public long? FileSize { get; set; }
        public DateTime? UploadedAt { get; set; }
        public bool IsVideo { get; set; }
    }
}

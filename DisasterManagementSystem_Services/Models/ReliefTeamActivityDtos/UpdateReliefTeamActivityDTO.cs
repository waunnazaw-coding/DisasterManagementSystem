using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisasterManagementSystem_Services.Models.ReliefTeamActivityDtos
{
    public class UpdateReliefTeamActivityDTO
    {
        public int Id { get; set; }
        public int ReliefTeamId { get; set; }
        public DateTime ActivityDate { get; set; }
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string? DetailedAddress { get; set; }
        public string ActivityType { get; set; } = null!;
        public int? PeopleHelped { get; set; }
        public string? ItemsDistributed { get; set; }
        public decimal? ExpenseAmount { get; set; }

        // Add these properties
        public List<int>? MediaIdsToDelete { get; set; }
        public List<IFormFile>? NewMediaFiles { get; set; }
    }
}

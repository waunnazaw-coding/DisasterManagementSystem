using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisasterManagementSystem_Services.Models.ReliefTeamActivityDtos
{
    public class CreateReliefTeamActivityDTO
    {
        public int ReliefTeamId { get; set; }
        public DateTime ActivityDate { get; set; }
        public string Title { get; set; } 
        public string Description { get; set; } 
        public string? DetailedAddress { get; set; }
        public string ActivityType { get; set; }
        public int? PeopleHelped { get; set; }
        public string? ItemsDistributed { get; set; }
        public decimal? ExpenseAmount { get; set; }
        public List<IFormFile> MediaFiles { get; set; } = new List<IFormFile>();
    }
}

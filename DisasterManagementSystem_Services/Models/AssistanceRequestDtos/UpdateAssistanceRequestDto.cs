using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisasterManagementSystem_Services.Models.AssistanceRequestDtos
{
    public class UpdateAssistanceRequestDto
    {
        public int? DisasterEventId { get; set; }
        public int? DisasterReportId { get; set; }
        [Range(0, int.MaxValue, ErrorMessage = "Location ID must be positive or 0 for null")]
        public int? LocationId { get; set; }
        public string SupportType { get; set; }
        public int? Quantity { get; set; }
        public string Unit { get; set; }
        public string Description { get; set; }
        public string Priority { get; set; }
        public string ContactName { get; set; }
        public string Email { get; set; }
        public string ContactPhone { get; set; }
        public string DetailedAddress { get; set; }
    }
}

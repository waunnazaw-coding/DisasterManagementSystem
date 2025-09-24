using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisasterManagementSystem_Services.Models.AssistanceRequestDtos
{
    namespace DisasterManagementSystem_Service.Models.Dtos
    {
        public class CreateAssistanceRequestDto
        {
            public int? DisasterEventId { get; set; }
            public int? DisasterReportId { get; set; }
            public int? LocationId { get; set; }
            public string SupportType { get; set; }
            public int? Quantity { get; set; }
            public string Unit { get; set; }
            public string Description { get; set; }
            public string Priority { get; set; } = "Medium";
            public string ContactName { get; set; }
            public string Email { get; set; }
            public string ContactPhone { get; set; }
            public string DetailedAddress { get; set; }
        }
    }
}

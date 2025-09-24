using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisasterManagementSystem_Services.Models.AssistanceRequestDtos
{
    namespace DisasterManagementSystem_Service.Models.Dtos
    {
        public class AssistanceRequestDto
        {
            public int Id { get; set; }
            public int? DisasterEventId { get; set; }
            public string? DisasterEventName { get; set; }
            public int? DisasterReportId { get; set; }
            public Guid? UserId { get; set; }
            public string? UserName { get; set; }
            public int? LocationId { get; set; }
            public string? LocationName { get; set; }
            public string SupportType { get; set; }
            public int? Quantity { get; set; }
            public string? Unit { get; set; }
            public string? Description { get; set; }
            public string Priority { get; set; }
            public string Status { get; set; }
            public string? ContactName { get; set; }
            public string? Email { get; set; }
            public string? ContactPhone { get; set; }
            public string? DetailedAddress { get; set; }
            public DateTime? CreatedAt { get; set; }
            public DateTime? UpdatedAt { get; set; }
            public DateTime? FulfilledAt { get; set; }
            public List<RequestAssignmentDto> Assignments { get; set; } = new();
        }
    }
}

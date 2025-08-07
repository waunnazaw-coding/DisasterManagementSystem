using DisasterManagementSystem_Services.Models.AssistanceRequestDtos.DisasterManagementSystem_Service.Models.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisasterManagementSystem_Services.Models
{
    public class RequestAssignmentDto
    {
        public int Id { get; set; }
        public int AssistanceRequestId { get; set; }
        public AssistanceRequestDto? RequestDetails { get; set; }
        public int ReliefTeamId { get; set; }
        public string? ReliefTeamName { get; set; }
        public Guid? AssignedById { get; set; }
        public string? AssignedByName { get; set; }
        public DateTime? AssignedAt { get; set; }
        public string Status { get; set; } = "Assigned";
        public string Priority { get; set; } = "Medium";
        public string? Notes { get; set; }
        public DateTime? CompletedAt { get; set; }
        public Guid? LastUpdatedById { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class CreateRequestAssignmentDto
    {
        public int AssistanceRequestId { get; set; }
        public int ReliefTeamId { get; set; }
        public string Priority { get; set; } = "Medium";
        public string? Notes { get; set; }
    }

    public class UpdateAssignmentStatusDto
    {
        public string Status { get; set; } = "Assigned";
        public string? Notes { get; set; }
    }
}

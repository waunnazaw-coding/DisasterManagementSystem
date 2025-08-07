using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisasterManagementSystem_Services.Models.NotificationDto.cs
{
    public class NotificationDto
    {
        public int Id { get; set; }
        public Guid UserId { get; set; }
        public string Message { get; set; }
        public bool IsRead { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string Type { get; set; }
        public int? RelatedEntityId { get; set; }
        public string Status { get; set; }
    }
}

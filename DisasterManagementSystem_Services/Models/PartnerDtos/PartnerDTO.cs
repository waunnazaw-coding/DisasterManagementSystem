using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisasterManagementSystem_Services.Models.PartnerDtos
{
    public class PartnerDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? ContactName { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public string? Website { get; set; }
        public string? Notes { get; set; }
        public bool IsPublic { get; set; }
        public string Status { get; set; } = "Active";
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string? LogoUrl { get; set; }
    }
}

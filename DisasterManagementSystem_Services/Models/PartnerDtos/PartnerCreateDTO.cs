using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisasterManagementSystem_Services.Models.PartnerDtos
{
    public class PartnerCreateDTO
    {
        [Required]
        public string Name { get; set; } = null!;

        public string? ContactName { get; set; }

        [EmailAddress]
        public string? Email { get; set; }

        public string? Phone { get; set; }

        public string? Address { get; set; }

        public string? Website { get; set; }

        public string? Notes { get; set; }

        public bool IsPublic { get; set; }

        public IFormFile? LogoFile { get; set; }
    }
}

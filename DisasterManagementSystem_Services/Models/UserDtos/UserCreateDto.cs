using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisasterManagementSystem_Services.Models.UserDtos
{
    public class UserCreateDto
    {
        [Required, StringLength(100)]
        public string Name { get; set; } = null!;

        [Required, EmailAddress]
        public string Email { get; set; } = null!;

        [Required]
        public string Role { get; set; } = null!;

        public string? Password { get; set; }
        public string? AuthProvider { get; set; }
        public string? ExternalId { get; set; }
    }
}

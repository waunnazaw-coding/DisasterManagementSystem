using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisasterManagementSystem_Services.Models.UserDtos
{
    public class UserUpdateDto
    {
        [Required]
        public Guid Id { get; set; }

        [StringLength(100)]
        public string? Name { get; set; }

        [EmailAddress]
        public string? Email { get; set; }

        [RegularExpression("^(User|Admin|Org|SysAdmin|ReliefTeam)$")]
        public string? Role { get; set; }

        [RegularExpression("^(Active|Blacklisted)$")]
        public string? Status { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;

namespace DisasterManagementSystem_Services.Models.AuthDtos;

public class ResetPasswordRequestDto
{
    [Required, EmailAddress]
    public string Email { get; set; }

    [Required]
    public string Token { get; set; }

    [Required, MinLength(6)]
    public string NewPassword { get; set; }
}
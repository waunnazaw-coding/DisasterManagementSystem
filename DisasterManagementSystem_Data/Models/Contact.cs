using System;
using System.Collections.Generic;

namespace DisasterManagementSystem_Data.Models;

public partial class Contact
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Phone { get; set; }

    public string Email { get; set; } = null!;

    public string Message { get; set; } = null!;

    public DateTime? SubmissionDate { get; set; }
}

using Microsoft.AspNetCore.Identity;

namespace SmartStudent.Core.Entities;

/// <summary>
/// Master identity user entity using native UUID (Guid) as Primary Key.
/// </summary>
public class ApplicationUser : IdentityUser<Guid>
{
    public ApplicationUser()
    {
        Id = Guid.CreateVersion7();
    }

    public string FullName { get; set; } = string.Empty;
    public string? ProfilePhotoUrl { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation Properties (1:1 optional profiles)
    public Student? StudentProfile { get; set; }
    public Teacher? TeacherProfile { get; set; }
}

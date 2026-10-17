using SmartStudent.Core.Common;
using SmartStudent.Core.Enums;

namespace SmartStudent.Core.Entities;

/// <summary>
/// Specialized entity representing a student profile linked 1:1 with ApplicationUser.
/// </summary>
public class Student : BaseEntity
{
    public string StudentNumber { get; set; } = string.Empty;
    public Guid ApplicationUserId { get; set; }
    public Guid? ClassRoomId { get; set; }
    public DateOnly DateOfBirth { get; set; }
    public Gender Gender { get; set; }
    public string? Address { get; set; }
    public DateOnly EnrollmentDate { get; set; }

    // Navigation Properties
    public ApplicationUser User { get; set; } = null!;
    public ClassRoom? ClassRoom { get; set; }
    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    public ICollection<Grade> Grades { get; set; } = new List<Grade>();
    public ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
}

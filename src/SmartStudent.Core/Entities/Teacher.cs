using SmartStudent.Core.Common;

namespace SmartStudent.Core.Entities;

/// <summary>
/// Specialized entity representing a teacher profile linked 1:1 with ApplicationUser.
/// </summary>
public class Teacher : BaseEntity
{
    public string EmployeeNumber { get; set; } = string.Empty;
    public Guid ApplicationUserId { get; set; }
    public string Department { get; set; } = string.Empty;
    public string Specialization { get; set; } = string.Empty;
    public DateOnly HireDate { get; set; }

    // Navigation Properties
    public ApplicationUser User { get; set; } = null!;
    public ICollection<Course> Courses { get; set; } = new List<Course>();
    public ICollection<Grade> GradedGrades { get; set; } = new List<Grade>();
    public ICollection<Attendance> MarkedAttendances { get; set; } = new List<Attendance>();
}

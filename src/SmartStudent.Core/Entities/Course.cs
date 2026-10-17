using SmartStudent.Core.Common;

namespace SmartStudent.Core.Entities;

/// <summary>
/// Specialized entity representing an academic course/subject.
/// </summary>
public class Course : BaseEntity
{
    public string CourseCode { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int CreditHours { get; set; }
    public Guid? TeacherId { get; set; }

    // Navigation Properties
    public Teacher? Teacher { get; set; }
    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    public ICollection<Grade> Grades { get; set; } = new List<Grade>();
    public ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
}

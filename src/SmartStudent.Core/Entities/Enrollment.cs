using SmartStudent.Core.Common;
using SmartStudent.Core.Enums;

namespace SmartStudent.Core.Entities;

/// <summary>
/// Transaction entity representing a student's enrollment in a course for a specific academic semester.
/// </summary>
public class Enrollment : BaseEntity
{
    public Guid StudentId { get; set; }
    public Guid CourseId { get; set; }
    public int AcademicYear { get; set; }
    public Semester Semester { get; set; }
    public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    public Student Student { get; set; } = null!;
    public Course Course { get; set; } = null!;
    public ICollection<Grade> Grades { get; set; } = new List<Grade>();
    public ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
}

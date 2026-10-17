using SmartStudent.Core.Common;
using SmartStudent.Core.Enums;

namespace SmartStudent.Core.Entities;

/// <summary>
/// Transaction entity representing a student's daily attendance record for a course.
/// </summary>
public class Attendance : BaseEntity
{
    public Guid EnrollmentId { get; set; }
    public Guid StudentId { get; set; }
    public Guid CourseId { get; set; }
    public Guid MarkedByTeacherId { get; set; }

    public DateOnly AttendanceDate { get; set; }
    public AttendanceStatus Status { get; set; }
    public string? Remarks { get; set; }

    // Navigation Properties
    public Enrollment Enrollment { get; set; } = null!;
    public Student Student { get; set; } = null!;
    public Course Course { get; set; } = null!;
    public Teacher MarkedByTeacher { get; set; } = null!;
}

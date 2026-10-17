using SmartStudent.Core.Common;

namespace SmartStudent.Core.Entities;

/// <summary>
/// Master entity representing an academic classroom/section (e.g., Grade 10 - Class A).
/// </summary>
public class ClassRoom : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public int GradeLevel { get; set; }
    public int Capacity { get; set; }
    public int AcademicYear { get; set; }

    // Navigation Properties
    public ICollection<Student> Students { get; set; } = new List<Student>();
}

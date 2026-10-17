using SmartStudent.Core.Common;
using SmartStudent.Core.Enums;
using SmartStudent.Core.Exceptions;

namespace SmartStudent.Core.Entities;

/// <summary>
/// Transaction entity representing a student's grade in a course assessment, with encapsulated score and letter calculation.
/// </summary>
public class Grade : BaseEntity
{
    public Guid EnrollmentId { get; set; }
    public Guid StudentId { get; set; }
    public Guid CourseId { get; set; }
    public Guid GradedByTeacherId { get; set; }

    public ExamType ExamType { get; set; }
    public DateOnly ExamDate { get; set; }
    public DateTime GradedAt { get; private set; } = DateTime.UtcNow;

    public decimal Score { get; private set; }
    public string GradeLetter { get; private set; } = string.Empty;
    public string? Remarks { get; set; }

    // Navigation Properties
    public Enrollment Enrollment { get; set; } = null!;
    public Student Student { get; set; } = null!;
    public Course Course { get; set; } = null!;
    public Teacher GradedByTeacher { get; set; } = null!;

    // Parameterless constructor required by EF Core
    public Grade()
    {
    }

    public Grade(
        Guid enrollmentId,
        Guid studentId,
        Guid courseId,
        Guid gradedByTeacherId,
        ExamType examType,
        DateOnly examDate,
        decimal score,
        string? remarks = null)
    {
        EnrollmentId = enrollmentId;
        StudentId = studentId;
        CourseId = courseId;
        GradedByTeacherId = gradedByTeacherId;
        ExamType = examType;
        ExamDate = examDate;
        Remarks = remarks;
        SetScore(score);
    }

    /// <summary>
    /// Encapsulates score validation (0-100), automatic GradeLetter calculation, and GradedAt timestamp update.
    /// </summary>
    public void SetScore(decimal score)
    {
        if (score < 0)
        {
            throw new BadRequestException("The number should be 0 or more.");
        }

        if (score > 100)
        {
            throw new BadRequestException("The maximum is 100.");
        }

        Score = score;
        GradeLetter = score switch
        {
            >= 95 => "A+",
            >= 85 => "A",
            >= 80 => "B",
            >= 50 => "C",
            _ => "F"
        };
        GradedAt = DateTime.UtcNow;
    }
}

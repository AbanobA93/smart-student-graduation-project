using SmartStudent.Core.Enums;

namespace SmartStudent.Application.DTOs;

// ==================== 1. CLASSROOM DTOs ====================
public class ClassRoomDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int GradeLevel { get; set; }
    public int Capacity { get; set; }
    public int AcademicYear { get; set; }
}

public class CreateClassRoomDto
{
    public string Name { get; set; } = string.Empty;
    public int GradeLevel { get; set; }
    public int Capacity { get; set; }
    public int AcademicYear { get; set; }
}

// ==================== 2. STUDENT & ENROLLMENT DTOs ====================
public class StudentDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string StudentNumber { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? ProfilePhotoUrl { get; set; }
    public Guid? ClassRoomId { get; set; }
    public string? ClassRoomName { get; set; }
    public DateOnly DateOfBirth { get; set; }
    public Gender Gender { get; set; }
    public string GenderName => Gender.ToString();
    public string? Address { get; set; }
    public DateOnly EnrollmentDate { get; set; }
}

public class UpdateStudentDto
{
    public string FullName { get; set; } = string.Empty;
    public DateOnly DateOfBirth { get; set; }
    public Gender Gender { get; set; }
    public string? Address { get; set; }
    public Guid? ClassRoomId { get; set; }
}

public class AssignStudentClassDto
{
    public Guid StudentId { get; set; }
    public Guid ClassRoomId { get; set; }
}

public class EnrollStudentCourseDto
{
    public Guid StudentId { get; set; }
    public Guid CourseId { get; set; }
    public int AcademicYear { get; set; }
    public Semester Semester { get; set; }
}

public class EnrollmentDto
{
    public Guid Id { get; set; }
    public Guid StudentId { get; set; }
    public string StudentNumber { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
    public Guid CourseId { get; set; }
    public string CourseCode { get; set; } = string.Empty;
    public string CourseTitle { get; set; } = string.Empty;
    public int AcademicYear { get; set; }
    public Semester Semester { get; set; }
    public string SemesterName => Semester.ToString();
    public DateTime EnrolledAt { get; set; }
}

// ==================== 3. TEACHER & COURSE DTOs ====================
public class TeacherDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string EmployeeNumber { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string Specialization { get; set; } = string.Empty;
    public DateOnly HireDate { get; set; }
}

public class CourseDto
{
    public Guid Id { get; set; }
    public string CourseCode { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int CreditHours { get; set; }
    public Guid? TeacherId { get; set; }
    public string? TeacherName { get; set; }
}

public class CreateCourseDto
{
    public string CourseCode { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int CreditHours { get; set; }
    public Guid? TeacherId { get; set; }
}

public class UpdateCourseDto
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int CreditHours { get; set; }
    public Guid? TeacherId { get; set; }
}

public class AssignTeacherToCourseDto
{
    public Guid CourseId { get; set; }
    public Guid TeacherId { get; set; }
}

// ==================== 4. GRADE DTOs ====================
public class GradeDto
{
    public Guid Id { get; set; }
    public Guid EnrollmentId { get; set; }
    public Guid StudentId { get; set; }
    public string StudentNumber { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
    public Guid CourseId { get; set; }
    public string CourseCode { get; set; } = string.Empty;
    public string CourseTitle { get; set; } = string.Empty;
    public ExamType ExamType { get; set; }
    public string ExamTypeName => ExamType.ToString();
    public DateOnly ExamDate { get; set; }
    public DateTime GradedAt { get; set; }
    public decimal Score { get; set; }
    public string GradeLetter { get; set; } = string.Empty;
    public string? Remarks { get; set; }
    public Guid GradedByTeacherId { get; set; }
}

public class CreateGradeDto
{
    public Guid StudentId { get; set; }
    public Guid CourseId { get; set; }
    public ExamType ExamType { get; set; }
    public DateOnly ExamDate { get; set; }
    public decimal Score { get; set; }
    public string? Remarks { get; set; }
}

public class UpdateGradeDto
{
    public decimal Score { get; set; }
    public DateOnly ExamDate { get; set; }
    public string? Remarks { get; set; }
}

// ==================== 5. ATTENDANCE DTOs ====================
public class AttendanceDto
{
    public Guid Id { get; set; }
    public Guid EnrollmentId { get; set; }
    public Guid StudentId { get; set; }
    public string StudentNumber { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
    public Guid CourseId { get; set; }
    public string CourseCode { get; set; } = string.Empty;
    public string CourseTitle { get; set; } = string.Empty;
    public DateOnly AttendanceDate { get; set; }
    public AttendanceStatus Status { get; set; }
    public string StatusName => Status.ToString();
    public string? Remarks { get; set; }
    public Guid MarkedByTeacherId { get; set; }
}

public class MarkAttendanceDto
{
    public Guid StudentId { get; set; }
    public Guid CourseId { get; set; }
    public DateOnly AttendanceDate { get; set; }
    public AttendanceStatus Status { get; set; }
    public string? Remarks { get; set; }
}

// ==================== 6. ADMIN DASHBOARD DTOs ====================
public class UserSummaryDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ChangeUserRoleDto
{
    public Guid UserId { get; set; }
    public UserRole NewRole { get; set; }
}

public class SystemStatisticsDto
{
    public int TotalUsers { get; set; }
    public int TotalStudents { get; set; }
    public int TotalTeachers { get; set; }
    public int TotalCourses { get; set; }
    public int TotalClassRooms { get; set; }
    public int TotalActiveEnrollments { get; set; }
    public decimal AverageGradeScore { get; set; }
    public decimal OverallAttendancePercentage { get; set; }
}

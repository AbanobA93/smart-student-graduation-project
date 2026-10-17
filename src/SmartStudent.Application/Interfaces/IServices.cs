using SmartStudent.Application.Common;
using SmartStudent.Application.DTOs;

namespace SmartStudent.Application.Interfaces;

public interface IClassRoomService
{
    Task<IReadOnlyList<ClassRoomDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ClassRoomDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ClassRoomDto> CreateAsync(CreateClassRoomDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}

public interface IStudentService
{
    Task<PaginatedResult<StudentDto>> GetPagedStudentsAsync(PaginationParams pagination, CancellationToken cancellationToken = default);
    Task<StudentDto> GetStudentByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<StudentDto> UpdateStudentAsync(Guid id, UpdateStudentDto dto, CancellationToken cancellationToken = default);
    Task DeleteStudentAsync(Guid id, CancellationToken cancellationToken = default);
    Task AssignToClassRoomAsync(AssignStudentClassDto dto, CancellationToken cancellationToken = default);
    Task<EnrollmentDto> EnrollInCourseAsync(EnrollStudentCourseDto dto, CancellationToken cancellationToken = default);
    Task<string> UpdateProfilePhotoAsync(Guid studentId, string photoUrl, CancellationToken cancellationToken = default);
}

public interface ITeacherService
{
    Task<PaginatedResult<TeacherDto>> GetPagedTeachersAsync(PaginationParams pagination, CancellationToken cancellationToken = default);
    Task<TeacherDto> GetTeacherByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task DeleteTeacherAsync(Guid id, CancellationToken cancellationToken = default);
}

public interface ICourseService
{
    Task<PaginatedResult<CourseDto>> GetPagedCoursesAsync(PaginationParams pagination, CancellationToken cancellationToken = default);
    Task<CourseDto> GetCourseByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<CourseDto> CreateCourseAsync(CreateCourseDto dto, CancellationToken cancellationToken = default);
    Task<CourseDto> UpdateCourseAsync(Guid id, UpdateCourseDto dto, CancellationToken cancellationToken = default);
    Task DeleteCourseAsync(Guid id, CancellationToken cancellationToken = default);
    Task<CourseDto> AssignTeacherAsync(AssignTeacherToCourseDto dto, CancellationToken cancellationToken = default);
}

public interface IGradeService
{
    /// <summary>
    /// Teacher adds a grade for a student in their assigned course.
    /// Verifies that the course is taught by teacherProfileId and that the student is enrolled.
    /// </summary>
    Task<GradeDto> AddGradeAsync(Guid teacherProfileId, CreateGradeDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Teacher updates an existing grade in their assigned course.
    /// </summary>
    Task<GradeDto> UpdateGradeAsync(Guid teacherProfileId, Guid gradeId, UpdateGradeDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Teacher views all grades for a specific course they teach.
    /// </summary>
    Task<IReadOnlyList<GradeDto>> GetCourseGradesForTeacherAsync(Guid teacherProfileId, Guid courseId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Student views ONLY their own grades (studentProfileId extracted from JWT token).
    /// </summary>
    Task<IReadOnlyList<GradeDto>> GetMyGradesForStudentAsync(Guid studentProfileId, CancellationToken cancellationToken = default);
}

public interface IAttendanceService
{
    /// <summary>
    /// Teacher marks or updates daily attendance for a student in their assigned course.
    /// </summary>
    Task<AttendanceDto> MarkAttendanceAsync(Guid teacherProfileId, MarkAttendanceDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Teacher views attendance records for a course they teach.
    /// </summary>
    Task<IReadOnlyList<AttendanceDto>> GetCourseAttendanceForTeacherAsync(Guid teacherProfileId, Guid courseId, DateOnly? date = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Student views ONLY their own attendance history (studentProfileId extracted from JWT token).
    /// </summary>
    Task<IReadOnlyList<AttendanceDto>> GetMyAttendanceForStudentAsync(Guid studentProfileId, CancellationToken cancellationToken = default);
}

public interface IAdminDashboardService
{
    Task<PaginatedResult<UserSummaryDto>> GetAllUsersAsync(PaginationParams pagination, CancellationToken cancellationToken = default);
    Task ChangeUserRoleAsync(ChangeUserRoleDto dto, CancellationToken cancellationToken = default);
    Task<SystemStatisticsDto> GetSystemStatisticsAsync(CancellationToken cancellationToken = default);
}

public interface IFileStorageService
{
    Task<string> SaveProfilePhotoAsync(Stream fileStream, string fileName, CancellationToken cancellationToken = default);
}

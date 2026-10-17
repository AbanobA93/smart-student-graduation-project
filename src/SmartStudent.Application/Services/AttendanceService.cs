using AutoMapper;
using SmartStudent.Application.DTOs;
using SmartStudent.Application.Interfaces;
using SmartStudent.Core.Entities;
using SmartStudent.Core.Exceptions;

namespace SmartStudent.Application.Services;

/// <summary>
/// Manages daily course attendance with strict Teacher ownership checks and fast AsNoTracking student history queries.
/// </summary>
public class AttendanceService : IAttendanceService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public AttendanceService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<AttendanceDto> MarkAttendanceAsync(
        Guid teacherProfileId,
        MarkAttendanceDto dto,
        CancellationToken cancellationToken = default)
    {
        var course = await _unitOfWork.Courses.GetByIdAsync(dto.CourseId, asNoTracking: true, cancellationToken: cancellationToken)
            ?? throw new NotFoundException(nameof(Course), dto.CourseId);

        if (course.TeacherId != teacherProfileId)
        {
            throw new ForbiddenAccessException("You are not the assigned teacher for this course.");
        }

        var enrollment = await _unitOfWork.Enrollments.FirstOrDefaultAsync(
            e => e.StudentId == dto.StudentId && e.CourseId == dto.CourseId,
            asNoTracking: true,
            cancellationToken: cancellationToken)
            ?? throw new BadRequestException("Cannot mark attendance: The student is not enrolled in this course.");

        var alreadyMarked = await _unitOfWork.Attendances.AnyAsync(
            a => a.EnrollmentId == enrollment.Id && a.AttendanceDate == dto.AttendanceDate,
            cancellationToken);

        if (alreadyMarked)
        {
            throw new ConflictException($"Attendance for this student on {dto.AttendanceDate} has already been recorded.");
        }

        var attendance = new Attendance
        {
            EnrollmentId = enrollment.Id,
            StudentId = dto.StudentId,
            CourseId = dto.CourseId,
            MarkedByTeacherId = teacherProfileId,
            AttendanceDate = dto.AttendanceDate,
            Status = dto.Status,
            Remarks = dto.Remarks
        };

        await _unitOfWork.Attendances.AddAsync(attendance, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var saved = await _unitOfWork.Attendances.GetByIdAsync(
            attendance.Id,
            asNoTracking: true,
            cancellationToken: cancellationToken,
            a => a.Student.User,
            a => a.Course);

        return _mapper.Map<AttendanceDto>(saved ?? attendance);
    }

    public async Task<IReadOnlyList<AttendanceDto>> GetCourseAttendanceForTeacherAsync(
        Guid teacherProfileId,
        Guid courseId,
        DateOnly? date = null,
        CancellationToken cancellationToken = default)
    {
        var course = await _unitOfWork.Courses.GetByIdAsync(courseId, asNoTracking: true, cancellationToken: cancellationToken)
            ?? throw new NotFoundException(nameof(Course), courseId);

        if (course.TeacherId != teacherProfileId)
        {
            throw new ForbiddenAccessException("You are not the assigned teacher for this course.");
        }

        var records = await _unitOfWork.Attendances.GetAllAsync(
            predicate: a => a.CourseId == courseId && (!date.HasValue || a.AttendanceDate == date.Value),
            asNoTracking: true,
            cancellationToken: cancellationToken,
            a => a.Student.User,
            a => a.Course);

        return _mapper.Map<IReadOnlyList<AttendanceDto>>(records);
    }

    public async Task<IReadOnlyList<AttendanceDto>> GetMyAttendanceForStudentAsync(
        Guid studentProfileId,
        CancellationToken cancellationToken = default)
    {
        var records = await _unitOfWork.Attendances.GetAllAsync(
            predicate: a => a.StudentId == studentProfileId,
            asNoTracking: true,
            cancellationToken: cancellationToken,
            a => a.Student.User,
            a => a.Course);

        return _mapper.Map<IReadOnlyList<AttendanceDto>>(records);
    }
}

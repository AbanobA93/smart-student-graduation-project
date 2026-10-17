using AutoMapper;
using SmartStudent.Application.DTOs;
using SmartStudent.Application.Interfaces;
using SmartStudent.Core.Entities;
using SmartStudent.Core.Exceptions;

namespace SmartStudent.Application.Services;

/// <summary>
/// Manages student grades with strict Teacher course-ownership security and encapsulated score/letter calculation.
/// </summary>
public class GradeService : IGradeService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GradeService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<GradeDto> AddGradeAsync(
        Guid teacherProfileId,
        CreateGradeDto dto,
        CancellationToken cancellationToken = default)
    {
        var course = await _unitOfWork.Courses.GetByIdAsync(dto.CourseId, asNoTracking: true, cancellationToken: cancellationToken)
            ?? throw new NotFoundException(nameof(Course), dto.CourseId);

        // Anti-IDOR / Resource Ownership Security: Teacher can only grade courses assigned to them
        if (course.TeacherId != teacherProfileId)
        {
            throw new ForbiddenAccessException("You are not the assigned teacher for this course.");
        }

        // Verify student is enrolled in this course
        var enrollment = await _unitOfWork.Enrollments.FirstOrDefaultAsync(
            e => e.StudentId == dto.StudentId && e.CourseId == dto.CourseId,
            asNoTracking: true,
            cancellationToken: cancellationToken)
            ?? throw new BadRequestException("Cannot add grade: The student is not enrolled in this course.");

        var duplicateGrade = await _unitOfWork.Grades.AnyAsync(
            g => g.EnrollmentId == enrollment.Id &&
                 g.ExamType == dto.ExamType &&
                 g.ExamDate == dto.ExamDate,
            cancellationToken);

        if (duplicateGrade)
        {
            throw new ConflictException($"A {dto.ExamType} grade for this student on {dto.ExamDate} already exists.");
        }

        // Constructor calls SetScore(dto.Score) which validates 0..100, computes GradeLetter (F..A+), and stamps GradedAt
        var grade = new Grade(
            enrollmentId: enrollment.Id,
            studentId: dto.StudentId,
            courseId: dto.CourseId,
            gradedByTeacherId: teacherProfileId,
            examType: dto.ExamType,
            examDate: dto.ExamDate,
            score: dto.Score,
            remarks: dto.Remarks);

        await _unitOfWork.Grades.AddAsync(grade, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var saved = await _unitOfWork.Grades.GetByIdAsync(
            grade.Id,
            asNoTracking: true,
            cancellationToken: cancellationToken,
            g => g.Student.User,
            g => g.Course);

        return _mapper.Map<GradeDto>(saved ?? grade);
    }

    public async Task<GradeDto> UpdateGradeAsync(
        Guid teacherProfileId,
        Guid gradeId,
        UpdateGradeDto dto,
        CancellationToken cancellationToken = default)
    {
        var grade = await _unitOfWork.Grades.GetByIdAsync(
            gradeId,
            asNoTracking: false,
            cancellationToken: cancellationToken,
            g => g.Course,
            g => g.Student.User)
            ?? throw new NotFoundException(nameof(Grade), gradeId);

        if (grade.Course.TeacherId != teacherProfileId)
        {
            throw new ForbiddenAccessException("You are not the assigned teacher for this course.");
        }

        grade.ExamDate = dto.ExamDate;
        grade.Remarks = dto.Remarks;
        grade.SetScore(dto.Score); // Re-validates 0..100, recalculates GradeLetter, and updates GradedAt

        _unitOfWork.Grades.Update(grade);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<GradeDto>(grade);
    }

    public async Task<IReadOnlyList<GradeDto>> GetCourseGradesForTeacherAsync(
        Guid teacherProfileId,
        Guid courseId,
        CancellationToken cancellationToken = default)
    {
        var course = await _unitOfWork.Courses.GetByIdAsync(courseId, asNoTracking: true, cancellationToken: cancellationToken)
            ?? throw new NotFoundException(nameof(Course), courseId);

        if (course.TeacherId != teacherProfileId)
        {
            throw new ForbiddenAccessException("You are not the assigned teacher for this course.");
        }

        var grades = await _unitOfWork.Grades.GetAllAsync(
            predicate: g => g.CourseId == courseId,
            asNoTracking: true,
            cancellationToken: cancellationToken,
            g => g.Student.User,
            g => g.Course);

        return _mapper.Map<IReadOnlyList<GradeDto>>(grades);
    }

    public async Task<IReadOnlyList<GradeDto>> GetMyGradesForStudentAsync(
        Guid studentProfileId,
        CancellationToken cancellationToken = default)
    {
        var grades = await _unitOfWork.Grades.GetAllAsync(
            predicate: g => g.StudentId == studentProfileId,
            asNoTracking: true,
            cancellationToken: cancellationToken,
            g => g.Student.User,
            g => g.Course);

        return _mapper.Map<IReadOnlyList<GradeDto>>(grades);
    }
}

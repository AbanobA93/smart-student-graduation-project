using System.Linq.Expressions;
using AutoMapper;
using SmartStudent.Application.Common;
using SmartStudent.Application.DTOs;
using SmartStudent.Application.Interfaces;
using SmartStudent.Core.Entities;
using SmartStudent.Core.Exceptions;

namespace SmartStudent.Application.Services;

public class StudentService : IStudentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public StudentService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PaginatedResult<StudentDto>> GetPagedStudentsAsync(
        PaginationParams pagination,
        CancellationToken cancellationToken = default)
    {
        Expression<Func<Student, bool>>? predicate = null;
        if (!string.IsNullOrWhiteSpace(pagination.SearchTerm))
        {
            var term = pagination.SearchTerm.Trim().ToLower();
            predicate = s =>
                s.StudentNumber.ToLower().Contains(term) ||
                s.User.FullName.ToLower().Contains(term) ||
                (s.User.Email != null && s.User.Email.ToLower().Contains(term));
        }

        var paged = await _unitOfWork.Students.GetPagedAsync(
            pagination.PageNumber,
            pagination.PageSize,
            predicate: predicate,
            orderBy: null,
            asNoTracking: true,
            cancellationToken: cancellationToken,
            s => s.User,
            s => s.ClassRoom!);

        var dtoItems = _mapper.Map<IReadOnlyList<StudentDto>>(paged.Items);
        return new PaginatedResult<StudentDto>(dtoItems, paged.TotalCount, paged.PageNumber, paged.PageSize);
    }

    public async Task<StudentDto> GetStudentByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var student = await _unitOfWork.Students.GetByIdAsync(
            id,
            asNoTracking: true,
            cancellationToken: cancellationToken,
            s => s.User,
            s => s.ClassRoom!)
            ?? throw new NotFoundException(nameof(Student), id);

        return _mapper.Map<StudentDto>(student);
    }

    public async Task<StudentDto> UpdateStudentAsync(Guid id, UpdateStudentDto dto, CancellationToken cancellationToken = default)
    {
        var student = await _unitOfWork.Students.GetByIdAsync(
            id,
            asNoTracking: false,
            cancellationToken: cancellationToken,
            s => s.User,
            s => s.ClassRoom!)
            ?? throw new NotFoundException(nameof(Student), id);

        if (dto.ClassRoomId.HasValue)
        {
            var classExists = await _unitOfWork.ClassRooms.AnyAsync(c => c.Id == dto.ClassRoomId.Value, cancellationToken);
            if (!classExists)
            {
                throw new NotFoundException(nameof(ClassRoom), dto.ClassRoomId.Value);
            }
        }

        student.User.FullName = dto.FullName;
        student.User.UpdatedAt = DateTime.UtcNow;
        student.DateOfBirth = dto.DateOfBirth;
        student.Gender = dto.Gender;
        student.Address = dto.Address;
        student.ClassRoomId = dto.ClassRoomId;

        _unitOfWork.Students.Update(student);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<StudentDto>(student);
    }

    public async Task DeleteStudentAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var student = await _unitOfWork.Students.GetByIdAsync(
            id,
            asNoTracking: false,
            cancellationToken: cancellationToken,
            s => s.User)
            ?? throw new NotFoundException(nameof(Student), id);

        student.User.IsActive = false;
        _unitOfWork.Students.SoftDelete(student);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task AssignToClassRoomAsync(AssignStudentClassDto dto, CancellationToken cancellationToken = default)
    {
        var student = await _unitOfWork.Students.GetByIdAsync(dto.StudentId, asNoTracking: false, cancellationToken: cancellationToken)
            ?? throw new NotFoundException(nameof(Student), dto.StudentId);

        var classRoom = await _unitOfWork.ClassRooms.GetByIdAsync(dto.ClassRoomId, asNoTracking: true, cancellationToken: cancellationToken)
            ?? throw new NotFoundException(nameof(ClassRoom), dto.ClassRoomId);

        var currentCount = await _unitOfWork.Students.CountAsync(s => s.ClassRoomId == dto.ClassRoomId, cancellationToken);
        if (currentCount >= classRoom.Capacity)
        {
            throw new BadRequestException($"Classroom '{classRoom.Name}' has reached its maximum capacity ({classRoom.Capacity}).");
        }

        student.ClassRoomId = dto.ClassRoomId;
        _unitOfWork.Students.Update(student);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<EnrollmentDto> EnrollInCourseAsync(EnrollStudentCourseDto dto, CancellationToken cancellationToken = default)
    {
        var student = await _unitOfWork.Students.GetByIdAsync(
            dto.StudentId,
            asNoTracking: true,
            cancellationToken: cancellationToken,
            s => s.User)
            ?? throw new NotFoundException(nameof(Student), dto.StudentId);

        var course = await _unitOfWork.Courses.GetByIdAsync(
            dto.CourseId,
            asNoTracking: true,
            cancellationToken: cancellationToken)
            ?? throw new NotFoundException(nameof(Course), dto.CourseId);

        var alreadyEnrolled = await _unitOfWork.Enrollments.AnyAsync(
            e => e.StudentId == dto.StudentId &&
                 e.CourseId == dto.CourseId &&
                 e.AcademicYear == dto.AcademicYear &&
                 e.Semester == dto.Semester,
            cancellationToken);

        if (alreadyEnrolled)
        {
            throw new ConflictException($"Student is already enrolled in course '{course.CourseCode}' for {dto.Semester} {dto.AcademicYear}.");
        }

        var enrollment = new Enrollment
        {
            StudentId = dto.StudentId,
            CourseId = dto.CourseId,
            AcademicYear = dto.AcademicYear,
            Semester = dto.Semester,
            EnrolledAt = DateTime.UtcNow
        };

        await _unitOfWork.Enrollments.AddAsync(enrollment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new EnrollmentDto
        {
            Id = enrollment.Id,
            StudentId = student.Id,
            StudentNumber = student.StudentNumber,
            StudentName = student.User.FullName,
            CourseId = course.Id,
            CourseCode = course.CourseCode,
            CourseTitle = course.Title,
            AcademicYear = enrollment.AcademicYear,
            Semester = enrollment.Semester,
            EnrolledAt = enrollment.EnrolledAt
        };
    }

    public async Task<string> UpdateProfilePhotoAsync(Guid studentId, string photoUrl, CancellationToken cancellationToken = default)
    {
        var student = await _unitOfWork.Students.GetByIdAsync(
            studentId,
            asNoTracking: false,
            cancellationToken: cancellationToken,
            s => s.User)
            ?? throw new NotFoundException(nameof(Student), studentId);

        student.User.ProfilePhotoUrl = photoUrl;
        student.User.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return photoUrl;
    }
}

using System.Linq.Expressions;
using AutoMapper;
using SmartStudent.Application.Common;
using SmartStudent.Application.DTOs;
using SmartStudent.Application.Interfaces;
using SmartStudent.Core.Entities;
using SmartStudent.Core.Exceptions;

namespace SmartStudent.Application.Services;

public class CourseService : ICourseService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CourseService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PaginatedResult<CourseDto>> GetPagedCoursesAsync(
        PaginationParams pagination,
        CancellationToken cancellationToken = default)
    {
        Expression<Func<Course, bool>>? predicate = null;
        if (!string.IsNullOrWhiteSpace(pagination.SearchTerm))
        {
            var term = pagination.SearchTerm.Trim().ToLower();
            predicate = c =>
                c.CourseCode.ToLower().Contains(term) ||
                c.Title.ToLower().Contains(term);
        }

        var paged = await _unitOfWork.Courses.GetPagedAsync(
            pagination.PageNumber,
            pagination.PageSize,
            predicate: predicate,
            orderBy: null,
            asNoTracking: true,
            cancellationToken: cancellationToken,
            c => c.Teacher!.User);

        var items = _mapper.Map<IReadOnlyList<CourseDto>>(paged.Items);
        return new PaginatedResult<CourseDto>(items, paged.TotalCount, paged.PageNumber, paged.PageSize);
    }

    public async Task<CourseDto> GetCourseByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var course = await _unitOfWork.Courses.GetByIdAsync(
            id,
            asNoTracking: true,
            cancellationToken: cancellationToken,
            c => c.Teacher!.User)
            ?? throw new NotFoundException(nameof(Course), id);

        return _mapper.Map<CourseDto>(course);
    }

    public async Task<CourseDto> CreateCourseAsync(CreateCourseDto dto, CancellationToken cancellationToken = default)
    {
        var codeExists = await _unitOfWork.Courses.AnyAsync(c => c.CourseCode == dto.CourseCode, cancellationToken);
        if (codeExists)
        {
            throw new ConflictException($"Course with code '{dto.CourseCode}' already exists.");
        }

        if (dto.TeacherId.HasValue)
        {
            var teacherExists = await _unitOfWork.Teachers.AnyAsync(t => t.Id == dto.TeacherId.Value, cancellationToken);
            if (!teacherExists)
            {
                throw new NotFoundException(nameof(Teacher), dto.TeacherId.Value);
            }
        }

        var course = _mapper.Map<Course>(dto);
        await _unitOfWork.Courses.AddAsync(course, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await GetCourseByIdAsync(course.Id, cancellationToken);
    }

    public async Task<CourseDto> UpdateCourseAsync(Guid id, UpdateCourseDto dto, CancellationToken cancellationToken = default)
    {
        var course = await _unitOfWork.Courses.GetByIdAsync(id, asNoTracking: false, cancellationToken: cancellationToken)
            ?? throw new NotFoundException(nameof(Course), id);

        if (dto.TeacherId.HasValue)
        {
            var teacherExists = await _unitOfWork.Teachers.AnyAsync(t => t.Id == dto.TeacherId.Value, cancellationToken);
            if (!teacherExists)
            {
                throw new NotFoundException(nameof(Teacher), dto.TeacherId.Value);
            }
        }

        course.Title = dto.Title;
        course.Description = dto.Description;
        course.CreditHours = dto.CreditHours;
        course.TeacherId = dto.TeacherId;

        _unitOfWork.Courses.Update(course);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await GetCourseByIdAsync(course.Id, cancellationToken);
    }

    public async Task DeleteCourseAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var course = await _unitOfWork.Courses.GetByIdAsync(id, asNoTracking: false, cancellationToken: cancellationToken)
            ?? throw new NotFoundException(nameof(Course), id);

        _unitOfWork.Courses.SoftDelete(course);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<CourseDto> AssignTeacherAsync(AssignTeacherToCourseDto dto, CancellationToken cancellationToken = default)
    {
        var course = await _unitOfWork.Courses.GetByIdAsync(dto.CourseId, asNoTracking: false, cancellationToken: cancellationToken)
            ?? throw new NotFoundException(nameof(Course), dto.CourseId);

        var teacher = await _unitOfWork.Teachers.GetByIdAsync(dto.TeacherId, asNoTracking: true, cancellationToken: cancellationToken, t => t.User)
            ?? throw new NotFoundException(nameof(Teacher), dto.TeacherId);

        course.TeacherId = teacher.Id;
        _unitOfWork.Courses.Update(course);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await GetCourseByIdAsync(course.Id, cancellationToken);
    }
}

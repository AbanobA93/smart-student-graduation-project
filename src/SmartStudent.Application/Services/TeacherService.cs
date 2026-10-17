using System.Linq.Expressions;
using AutoMapper;
using SmartStudent.Application.Common;
using SmartStudent.Application.DTOs;
using SmartStudent.Application.Interfaces;
using SmartStudent.Core.Entities;
using SmartStudent.Core.Exceptions;

namespace SmartStudent.Application.Services;

public class TeacherService : ITeacherService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public TeacherService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PaginatedResult<TeacherDto>> GetPagedTeachersAsync(
        PaginationParams pagination,
        CancellationToken cancellationToken = default)
    {
        Expression<Func<Teacher, bool>>? predicate = null;
        if (!string.IsNullOrWhiteSpace(pagination.SearchTerm))
        {
            var term = pagination.SearchTerm.Trim().ToLower();
            predicate = t =>
                t.EmployeeNumber.ToLower().Contains(term) ||
                t.Department.ToLower().Contains(term) ||
                t.User.FullName.ToLower().Contains(term);
        }

        var paged = await _unitOfWork.Teachers.GetPagedAsync(
            pagination.PageNumber,
            pagination.PageSize,
            predicate: predicate,
            orderBy: null,
            asNoTracking: true,
            cancellationToken: cancellationToken,
            t => t.User);

        var items = _mapper.Map<IReadOnlyList<TeacherDto>>(paged.Items);
        return new PaginatedResult<TeacherDto>(items, paged.TotalCount, paged.PageNumber, paged.PageSize);
    }

    public async Task<TeacherDto> GetTeacherByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var teacher = await _unitOfWork.Teachers.GetByIdAsync(
            id,
            asNoTracking: true,
            cancellationToken: cancellationToken,
            t => t.User)
            ?? throw new NotFoundException(nameof(Teacher), id);

        return _mapper.Map<TeacherDto>(teacher);
    }

    public async Task DeleteTeacherAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var teacher = await _unitOfWork.Teachers.GetByIdAsync(
            id,
            asNoTracking: false,
            cancellationToken: cancellationToken,
            t => t.User)
            ?? throw new NotFoundException(nameof(Teacher), id);

        teacher.User.IsActive = false;
        _unitOfWork.Teachers.SoftDelete(teacher);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

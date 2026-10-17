using SmartStudent.Application.Interfaces;
using SmartStudent.Core.Entities;
using SmartStudent.Infrastructure.Data;

namespace SmartStudent.Infrastructure.Repositories;

/// <summary>
/// Unit of Work implementation managing entity repositories and atomic SaveChangesAsync commits.
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;

    private IGenericRepository<ClassRoom>? _classRooms;
    private IGenericRepository<Student>? _students;
    private IGenericRepository<Teacher>? _teachers;
    private IGenericRepository<Course>? _courses;
    private IGenericRepository<Enrollment>? _enrollments;
    private IGenericRepository<Grade>? _grades;
    private IGenericRepository<Attendance>? _attendances;

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
    }

    public IGenericRepository<ClassRoom> ClassRooms =>
        _classRooms ??= new GenericRepository<ClassRoom>(_context);

    public IGenericRepository<Student> Students =>
        _students ??= new GenericRepository<Student>(_context);

    public IGenericRepository<Teacher> Teachers =>
        _teachers ??= new GenericRepository<Teacher>(_context);

    public IGenericRepository<Course> Courses =>
        _courses ??= new GenericRepository<Course>(_context);

    public IGenericRepository<Enrollment> Enrollments =>
        _enrollments ??= new GenericRepository<Enrollment>(_context);

    public IGenericRepository<Grade> Grades =>
        _grades ??= new GenericRepository<Grade>(_context);

    public IGenericRepository<Attendance> Attendances =>
        _attendances ??= new GenericRepository<Attendance>(_context);

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}

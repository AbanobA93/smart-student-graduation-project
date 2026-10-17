using SmartStudent.Core.Entities;

namespace SmartStudent.Application.Interfaces;

/// <summary>
/// Unit of Work contract coordinating repositories and atomic database commits.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    IGenericRepository<ClassRoom> ClassRooms { get; }
    IGenericRepository<Student> Students { get; }
    IGenericRepository<Teacher> Teachers { get; }
    IGenericRepository<Course> Courses { get; }
    IGenericRepository<Enrollment> Enrollments { get; }
    IGenericRepository<Grade> Grades { get; }
    IGenericRepository<Attendance> Attendances { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

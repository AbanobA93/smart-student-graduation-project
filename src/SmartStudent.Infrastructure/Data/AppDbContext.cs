using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SmartStudent.Core.Common;
using SmartStudent.Core.Entities;

namespace SmartStudent.Infrastructure.Data;

/// <summary>
/// Application EF Core DbContext integrating ASP.NET Core Identity with native Guid keys
/// and automatic audit timestamping + soft-delete query filters.
/// </summary>
public class AppDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    // 1st: Master Tables (ApplicationUser is provided by IdentityDbContext.Users)
    public DbSet<ClassRoom> ClassRooms => Set<ClassRoom>();

    // 2nd: Specialized Entities
    public DbSet<Teacher> Teachers => Set<Teacher>();
    public DbSet<Student> Students => Set<Student>();
    public DbSet<Course> Courses => Set<Course>();

    // 3rd: Transaction Tables
    public DbSet<Enrollment> Enrollments => Set<Enrollment>();
    public DbSet<Grade> Grades => Set<Grade>();
    public DbSet<Attendance> Attendances => Set<Attendance>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Apply all IEntityTypeConfiguration<T> classes from this assembly
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        // Global Soft-Delete Query Filter for all BaseEntity tables
        builder.Entity<ClassRoom>().HasQueryFilter(x => !x.IsDeleted);
        builder.Entity<Teacher>().HasQueryFilter(x => !x.IsDeleted);
        builder.Entity<Student>().HasQueryFilter(x => !x.IsDeleted);
        builder.Entity<Course>().HasQueryFilter(x => !x.IsDeleted);
        builder.Entity<Enrollment>().HasQueryFilter(x => !x.IsDeleted);
        builder.Entity<Grade>().HasQueryFilter(x => !x.IsDeleted);
        builder.Entity<Attendance>().HasQueryFilter(x => !x.IsDeleted);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = now;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = now;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}

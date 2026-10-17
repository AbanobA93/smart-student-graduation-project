using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SmartStudent.Application.Common;
using SmartStudent.Application.DTOs;
using SmartStudent.Application.Interfaces;
using SmartStudent.Core.Entities;
using SmartStudent.Core.Enums;
using SmartStudent.Core.Exceptions;
using SmartStudent.Infrastructure.Data;

namespace SmartStudent.Infrastructure.Identity;

/// <summary>
/// Implements Admin Dashboard operations: Paginated user listing, role management, and system-wide statistics.
/// </summary>
public class AdminDashboardService : IAdminDashboardService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly AppDbContext _context;

    public AdminDashboardService(
        UserManager<ApplicationUser> userManager,
        AppDbContext context)
    {
        _userManager = userManager;
        _context = context;
    }

    public async Task<PaginatedResult<UserSummaryDto>> GetAllUsersAsync(
        PaginationParams pagination,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Users.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(pagination.SearchTerm))
        {
            var term = pagination.SearchTerm.Trim().ToLower();
            query = query.Where(u =>
                u.FullName.ToLower().Contains(term) ||
                (u.Email != null && u.Email.ToLower().Contains(term)));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var users = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((pagination.PageNumber - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToListAsync(cancellationToken);

        // Fetch roles in batch to avoid N+1 queries
        var userIds = users.Select(u => u.Id).ToList();
        var userRoleMap = await (
            from ur in _context.UserRoles.AsNoTracking()
            join r in _context.Roles.AsNoTracking() on ur.RoleId equals r.Id
            where userIds.Contains(ur.UserId)
            select new { ur.UserId, RoleName = r.Name ?? string.Empty }
        ).ToListAsync(cancellationToken);

        var items = users.Select(u => new UserSummaryDto
        {
            Id = u.Id,
            FullName = u.FullName,
            Email = u.Email ?? string.Empty,
            Role = userRoleMap.FirstOrDefault(x => x.UserId == u.Id)?.RoleName ?? "None",
            IsActive = u.IsActive,
            CreatedAt = u.CreatedAt
        }).ToList();

        return new PaginatedResult<UserSummaryDto>(items, totalCount, pagination.PageNumber, pagination.PageSize);
    }

    public async Task ChangeUserRoleAsync(ChangeUserRoleDto dto, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(dto.UserId.ToString())
            ?? throw new NotFoundException(nameof(ApplicationUser), dto.UserId);

        var currentRoles = await _userManager.GetRolesAsync(user);
        if (currentRoles.Count > 0)
        {
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
        }

        var newRoleName = dto.NewRole.ToString();
        await _userManager.AddToRoleAsync(user, newRoleName);

        // Ensure corresponding domain profile exists when switching to Student or Teacher
        if (dto.NewRole == UserRole.Student)
        {
            var hasStudentProfile = await _context.Students
                .IgnoreQueryFilters()
                .AnyAsync(s => s.ApplicationUserId == user.Id, cancellationToken);

            if (!hasStudentProfile)
            {
                _context.Students.Add(new Student
                {
                    ApplicationUserId = user.Id,
                    StudentNumber = $"STU-{DateTime.UtcNow:yyyy}-{Guid.NewGuid().ToString()[..6].ToUpper()}",
                    DateOfBirth = new DateOnly(2005, 1, 1),
                    Gender = Gender.Male,
                    EnrollmentDate = DateOnly.FromDateTime(DateTime.UtcNow)
                });
                await _context.SaveChangesAsync(cancellationToken);
            }
        }
        else if (dto.NewRole == UserRole.Teacher)
        {
            var hasTeacherProfile = await _context.Teachers
                .IgnoreQueryFilters()
                .AnyAsync(t => t.ApplicationUserId == user.Id, cancellationToken);

            if (!hasTeacherProfile)
            {
                _context.Teachers.Add(new Teacher
                {
                    ApplicationUserId = user.Id,
                    EmployeeNumber = $"TCH-{DateTime.UtcNow:yyyy}-{Guid.NewGuid().ToString()[..6].ToUpper()}",
                    Department = "General",
                    Specialization = "General",
                    HireDate = DateOnly.FromDateTime(DateTime.UtcNow)
                });
                await _context.SaveChangesAsync(cancellationToken);
            }
        }
    }

    public async Task<SystemStatisticsDto> GetSystemStatisticsAsync(CancellationToken cancellationToken = default)
    {
        var totalUsers = await _context.Users.AsNoTracking().CountAsync(cancellationToken);
        var totalStudents = await _context.Students.AsNoTracking().CountAsync(cancellationToken);
        var totalTeachers = await _context.Teachers.AsNoTracking().CountAsync(cancellationToken);
        var totalCourses = await _context.Courses.AsNoTracking().CountAsync(cancellationToken);
        var totalClassRooms = await _context.ClassRooms.AsNoTracking().CountAsync(cancellationToken);
        var totalEnrollments = await _context.Enrollments.AsNoTracking().CountAsync(cancellationToken);

        var avgGrade = await _context.Grades.AsNoTracking().AnyAsync(cancellationToken)
            ? await _context.Grades.AsNoTracking().AverageAsync(g => g.Score, cancellationToken)
            : 0m;

        var totalAttendanceRecords = await _context.Attendances.AsNoTracking().CountAsync(cancellationToken);
        var presentCount = totalAttendanceRecords > 0
            ? await _context.Attendances.AsNoTracking().CountAsync(
                a => a.Status == AttendanceStatus.Present || a.Status == AttendanceStatus.Late,
                cancellationToken)
            : 0;

        var attendancePercentage = totalAttendanceRecords > 0
            ? Math.Round((decimal)presentCount / totalAttendanceRecords * 100m, 2)
            : 0m;

        return new SystemStatisticsDto
        {
            TotalUsers = totalUsers,
            TotalStudents = totalStudents,
            TotalTeachers = totalTeachers,
            TotalCourses = totalCourses,
            TotalClassRooms = totalClassRooms,
            TotalActiveEnrollments = totalEnrollments,
            AverageGradeScore = Math.Round(avgGrade, 2),
            OverallAttendancePercentage = attendancePercentage
        };
    }
}

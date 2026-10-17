using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartStudent.Core.Entities;

namespace SmartStudent.Infrastructure.Data.Configurations;

public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.Property(u => u.FullName)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(u => u.ProfilePhotoUrl)
            .HasMaxLength(500);

        builder.HasIndex(u => u.Email)
            .IsUnique();
    }
}

public class ClassRoomConfiguration : IEntityTypeConfiguration<ClassRoom>
{
    public void Configure(EntityTypeBuilder<ClassRoom> builder)
    {
        builder.ToTable("ClassRooms");

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(100);

        // Unique Composite Index: A classroom name must be unique per academic year
        builder.HasIndex(c => new { c.Name, c.AcademicYear })
            .IsUnique();
    }
}

public class TeacherConfiguration : IEntityTypeConfiguration<Teacher>
{
    public void Configure(EntityTypeBuilder<Teacher> builder)
    {
        builder.ToTable("Teachers");

        builder.Property(t => t.EmployeeNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(t => t.Department)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(t => t.Specialization)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(t => t.EmployeeNumber)
            .IsUnique();

        builder.HasIndex(t => t.ApplicationUserId)
            .IsUnique();

        builder.HasOne(t => t.User)
            .WithOne(u => u.TeacherProfile)
            .HasForeignKey<Teacher>(t => t.ApplicationUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> builder)
    {
        builder.ToTable("Students");

        builder.Property(s => s.StudentNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(s => s.Address)
            .HasMaxLength(250);

        builder.HasIndex(s => s.StudentNumber)
            .IsUnique();

        builder.HasIndex(s => s.ApplicationUserId)
            .IsUnique();

        builder.HasIndex(s => s.ClassRoomId);

        builder.HasOne(s => s.User)
            .WithOne(u => u.StudentProfile)
            .HasForeignKey<Student>(s => s.ApplicationUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.ClassRoom)
            .WithMany(c => c.Students)
            .HasForeignKey(s => s.ClassRoomId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

public class CourseConfiguration : IEntityTypeConfiguration<Course>
{
    public void Configure(EntityTypeBuilder<Course> builder)
    {
        builder.ToTable("Courses");

        builder.Property(c => c.CourseCode)
            .IsRequired()
            .HasMaxLength(30);

        builder.Property(c => c.Title)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(c => c.Description)
            .HasMaxLength(500);

        builder.HasIndex(c => c.CourseCode)
            .IsUnique();

        builder.HasIndex(c => c.TeacherId);

        builder.HasOne(c => c.Teacher)
            .WithMany(t => t.Courses)
            .HasForeignKey(c => c.TeacherId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

public class EnrollmentConfiguration : IEntityTypeConfiguration<Enrollment>
{
    public void Configure(EntityTypeBuilder<Enrollment> builder)
    {
        builder.ToTable("Enrollments");

        // Composite Unique Index: Prevents enrolling the same student in the same course twice in the same semester
        builder.HasIndex(e => new { e.StudentId, e.CourseId, e.AcademicYear, e.Semester })
            .IsUnique();

        builder.HasOne(e => e.Student)
            .WithMany(s => s.Enrollments)
            .HasForeignKey(e => e.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Course)
            .WithMany(c => c.Enrollments)
            .HasForeignKey(e => e.CourseId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class GradeConfiguration : IEntityTypeConfiguration<Grade>
{
    public void Configure(EntityTypeBuilder<Grade> builder)
    {
        builder.ToTable("Grades");

        builder.Property(g => g.Score)
            .HasPrecision(5, 2);

        builder.Property(g => g.GradeLetter)
            .IsRequired()
            .HasMaxLength(5);

        builder.Property(g => g.Remarks)
            .HasMaxLength(300);

        // Composite Unique Index: Prevents duplicate exam grade entries on the same date for the same enrollment
        builder.HasIndex(g => new { g.EnrollmentId, g.ExamType, g.ExamDate })
            .IsUnique();

        // Fast Read Index for Student & Course lookups
        builder.HasIndex(g => new { g.StudentId, g.CourseId });

        builder.HasOne(g => g.Enrollment)
            .WithMany(e => e.Grades)
            .HasForeignKey(g => g.EnrollmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(g => g.Student)
            .WithMany(s => s.Grades)
            .HasForeignKey(g => g.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(g => g.Course)
            .WithMany(c => c.Grades)
            .HasForeignKey(g => g.CourseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(g => g.GradedByTeacher)
            .WithMany(t => t.GradedGrades)
            .HasForeignKey(g => g.GradedByTeacherId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class AttendanceConfiguration : IEntityTypeConfiguration<Attendance>
{
    public void Configure(EntityTypeBuilder<Attendance> builder)
    {
        builder.ToTable("Attendances");

        builder.Property(a => a.Remarks)
            .HasMaxLength(300);

        // Composite Unique Index: Prevents marking attendance twice on the same date for the same enrollment
        builder.HasIndex(a => new { a.EnrollmentId, a.AttendanceDate })
            .IsUnique();

        // Fast Read Index for Student & Course attendance history
        builder.HasIndex(a => new { a.StudentId, a.CourseId, a.AttendanceDate });

        builder.HasOne(a => a.Enrollment)
            .WithMany(e => e.Attendances)
            .HasForeignKey(a => a.EnrollmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.Student)
            .WithMany(s => s.Attendances)
            .HasForeignKey(a => a.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.Course)
            .WithMany(c => c.Attendances)
            .HasForeignKey(a => a.CourseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.MarkedByTeacher)
            .WithMany(t => t.MarkedAttendances)
            .HasForeignKey(a => a.MarkedByTeacherId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

using System.Linq.Expressions;
using AutoMapper;
using Moq;
using SmartStudent.Application.DTOs;
using SmartStudent.Application.Interfaces;
using SmartStudent.Application.Services;
using SmartStudent.Core.Entities;
using SmartStudent.Core.Enums;
using SmartStudent.Core.Exceptions;

namespace SmartStudent.UnitTests;

public class AttendanceServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IGenericRepository<Course>> _courseRepoMock;
    private readonly Mock<IGenericRepository<Enrollment>> _enrollmentRepoMock;
    private readonly Mock<IGenericRepository<Attendance>> _attendanceRepoMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly AttendanceService _attendanceService;

    public AttendanceServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _courseRepoMock = new Mock<IGenericRepository<Course>>();
        _enrollmentRepoMock = new Mock<IGenericRepository<Enrollment>>();
        _attendanceRepoMock = new Mock<IGenericRepository<Attendance>>();
        _mapperMock = new Mock<IMapper>();

        _unitOfWorkMock.Setup(u => u.Courses).Returns(_courseRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.Enrollments).Returns(_enrollmentRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.Attendances).Returns(_attendanceRepoMock.Object);

        _attendanceService = new AttendanceService(_unitOfWorkMock.Object, _mapperMock.Object);
    }

    [Fact]
    public async Task MarkAttendanceAsync_WhenDuplicateAttendanceOnSameDate_ShouldThrowConflictException()
    {
        // Arrange
        var teacherId = Guid.CreateVersion7();
        var courseId = Guid.CreateVersion7();
        var studentId = Guid.CreateVersion7();
        var enrollmentId = Guid.CreateVersion7();

        _courseRepoMock
            .Setup(r => r.GetByIdAsync(courseId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Course { Id = courseId, TeacherId = teacherId });

        _enrollmentRepoMock
            .Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<Enrollment, bool>>>(),
                true,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Enrollment { Id = enrollmentId, StudentId = studentId, CourseId = courseId });

        _attendanceRepoMock
            .Setup(r => r.AnyAsync(
                It.IsAny<Expression<Func<Attendance, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var dto = new MarkAttendanceDto
        {
            CourseId = courseId,
            StudentId = studentId,
            AttendanceDate = new DateOnly(2026, 10, 17),
            Status = AttendanceStatus.Present
        };

        // Act & Assert
        await Assert.ThrowsAsync<ConflictException>(() =>
            _attendanceService.MarkAttendanceAsync(teacherId, dto));
    }
}

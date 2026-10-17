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

public class GradeServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IGenericRepository<Course>> _courseRepoMock;
    private readonly Mock<IGenericRepository<Enrollment>> _enrollmentRepoMock;
    private readonly Mock<IGenericRepository<Grade>> _gradeRepoMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly GradeService _gradeService;

    public GradeServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _courseRepoMock = new Mock<IGenericRepository<Course>>();
        _enrollmentRepoMock = new Mock<IGenericRepository<Enrollment>>();
        _gradeRepoMock = new Mock<IGenericRepository<Grade>>();
        _mapperMock = new Mock<IMapper>();

        _unitOfWorkMock.Setup(u => u.Courses).Returns(_courseRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.Enrollments).Returns(_enrollmentRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.Grades).Returns(_gradeRepoMock.Object);

        _gradeService = new GradeService(_unitOfWorkMock.Object, _mapperMock.Object);
    }

    [Fact]
    public async Task AddGradeAsync_WhenTeacherDoesNotOwnCourse_ShouldThrowForbiddenAccessException()
    {
        // Arrange
        var loggedInTeacherId = Guid.CreateVersion7();
        var otherTeacherId = Guid.CreateVersion7();
        var courseId = Guid.CreateVersion7();

        _courseRepoMock
            .Setup(r => r.GetByIdAsync(courseId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Course { Id = courseId, TeacherId = otherTeacherId });

        var dto = new CreateGradeDto
        {
            CourseId = courseId,
            StudentId = Guid.CreateVersion7(),
            ExamType = ExamType.Midterm,
            ExamDate = new DateOnly(2026, 10, 17),
            Score = 90
        };

        // Act & Assert
        await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
            _gradeService.AddGradeAsync(loggedInTeacherId, dto));
    }

    [Fact]
    public async Task AddGradeAsync_WhenStudentNotEnrolledInCourse_ShouldThrowBadRequestException()
    {
        // Arrange
        var teacherId = Guid.CreateVersion7();
        var courseId = Guid.CreateVersion7();
        var studentId = Guid.CreateVersion7();

        _courseRepoMock
            .Setup(r => r.GetByIdAsync(courseId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Course { Id = courseId, TeacherId = teacherId });

        _enrollmentRepoMock
            .Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<Enrollment, bool>>>(),
                true,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Enrollment?)null);

        var dto = new CreateGradeDto
        {
            CourseId = courseId,
            StudentId = studentId,
            ExamType = ExamType.Midterm,
            ExamDate = new DateOnly(2026, 10, 17),
            Score = 88
        };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<BadRequestException>(() =>
            _gradeService.AddGradeAsync(teacherId, dto));
        Assert.Contains("not enrolled", ex.Message);
    }

    [Fact]
    public async Task AddGradeAsync_WhenScoreLessThanZero_ShouldThrowBadRequestWithZeroOrMoreMessage()
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

        var dto = new CreateGradeDto
        {
            CourseId = courseId,
            StudentId = studentId,
            ExamType = ExamType.Quiz,
            ExamDate = new DateOnly(2026, 10, 17),
            Score = -5
        };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<BadRequestException>(() =>
            _gradeService.AddGradeAsync(teacherId, dto));
        Assert.Equal("The number should be 0 or more.", ex.Message);
    }

    [Fact]
    public async Task AddGradeAsync_WhenScoreGreaterThan100_ShouldThrowBadRequestWithMaximum100Message()
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

        var dto = new CreateGradeDto
        {
            CourseId = courseId,
            StudentId = studentId,
            ExamType = ExamType.Final,
            ExamDate = new DateOnly(2026, 10, 17),
            Score = 105
        };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<BadRequestException>(() =>
            _gradeService.AddGradeAsync(teacherId, dto));
        Assert.Equal("The maximum is 100.", ex.Message);
    }

    [Fact]
    public async Task AddGradeAsync_WhenValid_ShouldSaveGradeAndCalculateGradeLetter()
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

        Grade? capturedGrade = null;
        _gradeRepoMock
            .Setup(r => r.AddAsync(It.IsAny<Grade>(), It.IsAny<CancellationToken>()))
            .Callback<Grade, CancellationToken>((g, _) => capturedGrade = g)
            .Returns(Task.CompletedTask);

        _mapperMock
            .Setup(m => m.Map<GradeDto>(It.IsAny<Grade>()))
            .Returns((Grade g) => new GradeDto
            {
                Id = g.Id,
                Score = g.Score,
                GradeLetter = g.GradeLetter,
                ExamDate = g.ExamDate,
                GradedAt = g.GradedAt
            });

        var dto = new CreateGradeDto
        {
            CourseId = courseId,
            StudentId = studentId,
            ExamType = ExamType.Final,
            ExamDate = new DateOnly(2026, 10, 17),
            Score = 96
        };

        // Act
        var result = await _gradeService.AddGradeAsync(teacherId, dto);

        // Assert
        Assert.NotNull(capturedGrade);
        Assert.Equal(96m, capturedGrade!.Score);
        Assert.Equal("A+", capturedGrade.GradeLetter);
        Assert.Equal("A+", result.GradeLetter);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}

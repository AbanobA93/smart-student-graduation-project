using SmartStudent.Core.Entities;
using SmartStudent.Core.Enums;
using SmartStudent.Core.Exceptions;

namespace SmartStudent.UnitTests;

public class GradeEntityTests
{
    [Theory]
    [InlineData(-0.01)]
    [InlineData(-1)]
    [InlineData(-50)]
    public void SetScore_WhenScoreIsLessThanZero_ShouldThrowBadRequestWithZeroOrMoreMessage(decimal invalidScore)
    {
        // Arrange
        var grade = new Grade();

        // Act & Assert
        var exception = Assert.Throws<BadRequestException>(() => grade.SetScore(invalidScore));
        Assert.Equal("The number should be 0 or more.", exception.Message);
        Assert.Equal(400, exception.StatusCode);
    }

    [Theory]
    [InlineData(100.01)]
    [InlineData(101)]
    [InlineData(150)]
    public void SetScore_WhenScoreIsGreaterThan100_ShouldThrowBadRequestWithMaximum100Message(decimal invalidScore)
    {
        // Arrange
        var grade = new Grade();

        // Act & Assert
        var exception = Assert.Throws<BadRequestException>(() => grade.SetScore(invalidScore));
        Assert.Equal("The maximum is 100.", exception.Message);
        Assert.Equal(400, exception.StatusCode);
    }

    [Theory]
    [InlineData(0, "F")]
    [InlineData(25.5, "F")]
    [InlineData(49.99, "F")]
    [InlineData(50, "C")]
    [InlineData(65, "C")]
    [InlineData(79.99, "C")]
    [InlineData(80, "B")]
    [InlineData(82.5, "B")]
    [InlineData(84.99, "B")]
    [InlineData(85, "A")]
    [InlineData(90, "A")]
    [InlineData(94.99, "A")]
    [InlineData(95, "A+")]
    [InlineData(98.5, "A+")]
    [InlineData(100, "A+")]
    public void SetScore_WhenScoreIsValid_ShouldAssignExpectedGradeLetterAndStampGradedAt(decimal score, string expectedLetter)
    {
        // Arrange
        var beforeTime = DateTime.UtcNow;
        var grade = new Grade(
            enrollmentId: Guid.CreateVersion7(),
            studentId: Guid.CreateVersion7(),
            courseId: Guid.CreateVersion7(),
            gradedByTeacherId: Guid.CreateVersion7(),
            examType: ExamType.Final,
            examDate: new DateOnly(2026, 10, 15),
            score: score);

        // Assert
        Assert.Equal(score, grade.Score);
        Assert.Equal(expectedLetter, grade.GradeLetter);
        Assert.True(grade.GradedAt >= beforeTime);
    }
}

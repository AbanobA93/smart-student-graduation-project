using FluentValidation;
using SmartStudent.Application.DTOs;

namespace SmartStudent.Application.Validators;

public class CreateClassRoomValidator : AbstractValidator<CreateClassRoomDto>
{
    public CreateClassRoomValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Classroom name is required.")
            .MaximumLength(100);

        RuleFor(x => x.GradeLevel)
            .InclusiveBetween(1, 12).WithMessage("Grade level must be between 1 and 12.");

        RuleFor(x => x.Capacity)
            .InclusiveBetween(1, 500).WithMessage("Capacity must be between 1 and 500.");

        RuleFor(x => x.AcademicYear)
            .InclusiveBetween(2020, 2100).WithMessage("Academic year must be valid.");
    }
}

public class CreateCourseValidator : AbstractValidator<CreateCourseDto>
{
    public CreateCourseValidator()
    {
        RuleFor(x => x.CourseCode)
            .NotEmpty().WithMessage("Course code is required.")
            .MaximumLength(30);

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Course title is required.")
            .MaximumLength(150);

        RuleFor(x => x.CreditHours)
            .InclusiveBetween(1, 6).WithMessage("Credit hours must be between 1 and 6.");
    }
}

public class CreateGradeValidator : AbstractValidator<CreateGradeDto>
{
    public CreateGradeValidator()
    {
        RuleFor(x => x.StudentId)
            .NotEmpty().WithMessage("StudentId is required.");

        RuleFor(x => x.CourseId)
            .NotEmpty().WithMessage("CourseId is required.");

        RuleFor(x => x.ExamType)
            .IsInEnum().WithMessage("Valid ExamType is required.");

        RuleFor(x => x.Score)
            .GreaterThanOrEqualTo(0).WithMessage("The number should be 0 or more.")
            .LessThanOrEqualTo(100).WithMessage("The maximum is 100.");
    }
}

public class UpdateGradeValidator : AbstractValidator<UpdateGradeDto>
{
    public UpdateGradeValidator()
    {
        RuleFor(x => x.Score)
            .GreaterThanOrEqualTo(0).WithMessage("The number should be 0 or more.")
            .LessThanOrEqualTo(100).WithMessage("The maximum is 100.");
    }
}

public class MarkAttendanceValidator : AbstractValidator<MarkAttendanceDto>
{
    public MarkAttendanceValidator()
    {
        RuleFor(x => x.StudentId)
            .NotEmpty().WithMessage("StudentId is required.");

        RuleFor(x => x.CourseId)
            .NotEmpty().WithMessage("CourseId is required.");

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Valid AttendanceStatus is required.");
    }
}

public class EnrollStudentCourseValidator : AbstractValidator<EnrollStudentCourseDto>
{
    public EnrollStudentCourseValidator()
    {
        RuleFor(x => x.StudentId).NotEmpty();
        RuleFor(x => x.CourseId).NotEmpty();
        RuleFor(x => x.AcademicYear).InclusiveBetween(2020, 2100);
        RuleFor(x => x.Semester).IsInEnum();
    }
}

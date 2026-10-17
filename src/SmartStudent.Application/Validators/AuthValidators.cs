using FluentValidation;
using SmartStudent.Application.DTOs.Auth;

namespace SmartStudent.Application.Validators;

public class LoginRequestValidator : AbstractValidator<LoginRequestDto>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.");

        RuleFor(x => x.RoleDomain)
            .IsInEnum().WithMessage("A valid RoleDomain (Admin = 1, Teacher = 2, Student = 3) is required.");
    }
}

public class PortalLoginRequestValidator : AbstractValidator<PortalLoginRequestDto>
{
    public PortalLoginRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.");
    }
}

public class RegisterStudentRequestValidator : AbstractValidator<RegisterStudentRequestDto>
{
    public RegisterStudentRequestValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Full name is required.")
            .MaximumLength(150).WithMessage("Full name cannot exceed 150 characters.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters long.");

        RuleFor(x => x.StudentNumber)
            .NotEmpty().WithMessage("Student number is required.")
            .MaximumLength(50).WithMessage("Student number cannot exceed 50 characters.");

        RuleFor(x => x.Gender)
            .IsInEnum().WithMessage("Valid gender (Male = 1, Female = 2) is required.");
    }
}

public class RegisterTeacherRequestValidator : AbstractValidator<RegisterTeacherRequestDto>
{
    public RegisterTeacherRequestValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Full name is required.")
            .MaximumLength(150).WithMessage("Full name cannot exceed 150 characters.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters long.");

        RuleFor(x => x.EmployeeNumber)
            .NotEmpty().WithMessage("Employee number is required.")
            .MaximumLength(50).WithMessage("Employee number cannot exceed 50 characters.");

        RuleFor(x => x.Department)
            .NotEmpty().WithMessage("Department is required.")
            .MaximumLength(100).WithMessage("Department cannot exceed 100 characters.");

        RuleFor(x => x.Specialization)
            .NotEmpty().WithMessage("Specialization is required.")
            .MaximumLength(100).WithMessage("Specialization cannot exceed 100 characters.");
    }
}

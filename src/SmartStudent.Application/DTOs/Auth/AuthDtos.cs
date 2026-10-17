using SmartStudent.Core.Enums;

namespace SmartStudent.Application.DTOs.Auth;

/// <summary>
/// Request DTO for domain-isolated login.
/// A user must log into their exact assigned RoleDomain (Admin, Teacher, or Student).
/// Cross-domain login attempts (e.g., Teacher logging into Admin or Student) are strictly rejected with HTTP 403.
/// </summary>
public class LoginRequestDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public UserRole RoleDomain { get; set; }
}

/// <summary>
/// Portal-specific login request (when RoleDomain is determined by the endpoint route, e.g., /api/auth/login/teacher).
/// </summary>
public class PortalLoginRequestDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

/// <summary>
/// Request DTO for registering a new Student account and domain profile.
/// </summary>
public class RegisterStudentRequestDto
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string StudentNumber { get; set; } = string.Empty;
    public DateOnly DateOfBirth { get; set; }
    public Gender Gender { get; set; }
    public string? Address { get; set; }
    public Guid? ClassRoomId { get; set; }
}

/// <summary>
/// Request DTO for registering a new Teacher account and domain profile.
/// </summary>
public class RegisterTeacherRequestDto
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string EmployeeNumber { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string Specialization { get; set; } = string.Empty;
    public DateOnly HireDate { get; set; }
}

/// <summary>
/// Response DTO returned after successful authentication or registration, containing the domain-bound JWT token.
/// </summary>
public class AuthResponseDto
{
    public Guid UserId { get; set; }
    public Guid? ProfileId { get; set; } // Student.Id or Teacher.Id (null for Admin)
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public UserRole RoleDomain { get; set; }
    public string RoleName => RoleDomain.ToString();
    public string AccessToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}

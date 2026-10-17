using SmartStudent.Application.DTOs.Auth;

namespace SmartStudent.Application.Interfaces;

/// <summary>
/// Authentication and Role-Domain Isolation service contract.
/// Enforces that Admin, Teacher, and Student accounts can only authenticate within their own domain.
/// Any cross-domain login attempt or invalid credential returns a generic "Wrong email or password." (401) to prevent enumeration.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Authenticates a user and verifies that their account belongs to the requested RoleDomain.
    /// Throws UnauthorizedException ("Wrong email or password.") if email, password, or RoleDomain does not match.
    /// </summary>
    Task<AuthResponseDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Registers a new Student user account along with their Student entity profile.
    /// </summary>
    Task<AuthResponseDto> RegisterStudentAsync(RegisterStudentRequestDto request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Registers a new Teacher user account along with their Teacher entity profile.
    /// </summary>
    Task<AuthResponseDto> RegisterTeacherAsync(RegisterTeacherRequestDto request, CancellationToken cancellationToken = default);
}

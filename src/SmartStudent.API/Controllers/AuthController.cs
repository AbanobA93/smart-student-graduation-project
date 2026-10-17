using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartStudent.Application.Common;
using SmartStudent.Application.DTOs.Auth;
using SmartStudent.Application.Interfaces;
using SmartStudent.Core.Enums;

namespace SmartStudent.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Unified domain-isolated login endpoint. Returns 401 "Wrong email or password." on any credential or domain mismatch.
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Login(
        [FromBody] LoginRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await _authService.LoginAsync(request, cancellationToken);
        return Ok(ApiResponse<AuthResponseDto>.Success(result, "Logged in successfully."));
    }

    /// <summary>
    /// Admin-only domain login portal. Rejects Teacher and Student accounts with "Wrong email or password.".
    /// </summary>
    [HttpPost("login/admin")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> LoginAdmin(
        [FromBody] PortalLoginRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await _authService.LoginAsync(new LoginRequestDto
        {
            Email = request.Email,
            Password = request.Password,
            RoleDomain = UserRole.Admin
        }, cancellationToken);

        return Ok(ApiResponse<AuthResponseDto>.Success(result, "Logged in to Admin domain successfully."));
    }

    /// <summary>
    /// Teacher-only domain login portal. Rejects Admin and Student accounts with "Wrong email or password.".
    /// </summary>
    [HttpPost("login/teacher")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> LoginTeacher(
        [FromBody] PortalLoginRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await _authService.LoginAsync(new LoginRequestDto
        {
            Email = request.Email,
            Password = request.Password,
            RoleDomain = UserRole.Teacher
        }, cancellationToken);

        return Ok(ApiResponse<AuthResponseDto>.Success(result, "Logged in to Teacher domain successfully."));
    }

    /// <summary>
    /// Student-only domain login portal. Rejects Admin and Teacher accounts with "Wrong email or password.".
    /// </summary>
    [HttpPost("login/student")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> LoginStudent(
        [FromBody] PortalLoginRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await _authService.LoginAsync(new LoginRequestDto
        {
            Email = request.Email,
            Password = request.Password,
            RoleDomain = UserRole.Student
        }, cancellationToken);

        return Ok(ApiResponse<AuthResponseDto>.Success(result, "Logged in to Student domain successfully."));
    }

    /// <summary>
    /// Registers a new Student user and Student profile.
    /// </summary>
    [HttpPost("register/student")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> RegisterStudent(
        [FromBody] RegisterStudentRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await _authService.RegisterStudentAsync(request, cancellationToken);
        return StatusCode(201, ApiResponse<AuthResponseDto>.Success(result, "Student registered successfully.", 201));
    }

    /// <summary>
    /// Registers a new Teacher user and Teacher profile (Admin only).
    /// </summary>
    [HttpPost("register/teacher")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> RegisterTeacher(
        [FromBody] RegisterTeacherRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await _authService.RegisterTeacherAsync(request, cancellationToken);
        return StatusCode(201, ApiResponse<AuthResponseDto>.Success(result, "Teacher registered successfully.", 201));
    }
}

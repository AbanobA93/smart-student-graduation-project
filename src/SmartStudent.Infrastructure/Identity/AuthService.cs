using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SmartStudent.Application.Common;
using SmartStudent.Application.DTOs.Auth;
using SmartStudent.Application.Interfaces;
using SmartStudent.Core.Entities;
using SmartStudent.Core.Enums;
using SmartStudent.Core.Exceptions;

namespace SmartStudent.Infrastructure.Identity;

/// <summary>
/// Implements JWT authentication and strict Role-Domain Isolation.
/// Any invalid email, invalid password, or mismatched RoleDomain returns the exact same 401 "Wrong email or password." error.
/// </summary>
public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUnitOfWork _unitOfWork;
    private readonly JwtSettings _jwtSettings;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        IUnitOfWork unitOfWork,
        IOptions<JwtSettings> jwtOptions)
    {
        _userManager = userManager;
        _unitOfWork = unitOfWork;
        _jwtSettings = jwtOptions.Value;
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null || !user.IsActive)
        {
            throw new UnauthorizedException("Wrong email or password.");
        }

        var isPasswordValid = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!isPasswordValid)
        {
            throw new UnauthorizedException("Wrong email or password.");
        }

        // Strict Domain Isolation Check:
        // Teacher cannot log into Admin or Student; Student cannot log into Admin or Teacher; Admin cannot log into Teacher or Student.
        var targetRoleName = request.RoleDomain.ToString();
        var belongsToTargetDomain = await _userManager.IsInRoleAsync(user, targetRoleName);
        if (!belongsToTargetDomain)
        {
            throw new UnauthorizedException("Wrong email or password.");
        }

        // Resolve specialized ProfileId (Student.Id or Teacher.Id) using AsNoTracking
        Guid? profileId = null;
        if (request.RoleDomain == UserRole.Student)
        {
            var student = await _unitOfWork.Students.FirstOrDefaultAsync(
                s => s.ApplicationUserId == user.Id,
                asNoTracking: true,
                cancellationToken: cancellationToken);

            if (student is null)
            {
                throw new UnauthorizedException("Wrong email or password.");
            }
            profileId = student.Id;
        }
        else if (request.RoleDomain == UserRole.Teacher)
        {
            var teacher = await _unitOfWork.Teachers.FirstOrDefaultAsync(
                t => t.ApplicationUserId == user.Id,
                asNoTracking: true,
                cancellationToken: cancellationToken);

            if (teacher is null)
            {
                throw new UnauthorizedException("Wrong email or password.");
            }
            profileId = teacher.Id;
        }

        return GenerateJwtTokenResponse(user, request.RoleDomain, profileId);
    }

    public async Task<AuthResponseDto> RegisterStudentAsync(RegisterStudentRequestDto request, CancellationToken cancellationToken = default)
    {
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser is not null)
        {
            throw new ConflictException($"Email '{request.Email}' is already registered.");
        }

        var studentNoExists = await _unitOfWork.Students.AnyAsync(
            s => s.StudentNumber == request.StudentNumber,
            cancellationToken);
        if (studentNoExists)
        {
            throw new ConflictException($"Student number '{request.StudentNumber}' already exists.");
        }

        if (request.ClassRoomId.HasValue)
        {
            var classExists = await _unitOfWork.ClassRooms.AnyAsync(
                c => c.Id == request.ClassRoomId.Value,
                cancellationToken);
            if (!classExists)
            {
                throw new NotFoundException(nameof(ClassRoom), request.ClassRoomId.Value);
            }
        }

        var user = new ApplicationUser
        {
            FullName = request.FullName,
            Email = request.Email,
            UserName = request.Email,
            IsActive = true
        };

        var createResult = await _userManager.CreateAsync(user, request.Password);
        if (!createResult.Succeeded)
        {
            var errors = createResult.Errors.Select(e => e.Description);
            throw new BadRequestException("Failed to create student user account.", errors);
        }

        await _userManager.AddToRoleAsync(user, UserRole.Student.ToString());

        var student = new Student
        {
            ApplicationUserId = user.Id,
            StudentNumber = request.StudentNumber,
            DateOfBirth = request.DateOfBirth,
            Gender = request.Gender,
            Address = request.Address,
            ClassRoomId = request.ClassRoomId,
            EnrollmentDate = DateOnly.FromDateTime(DateTime.UtcNow)
        };

        await _unitOfWork.Students.AddAsync(student, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return GenerateJwtTokenResponse(user, UserRole.Student, student.Id);
    }

    public async Task<AuthResponseDto> RegisterTeacherAsync(RegisterTeacherRequestDto request, CancellationToken cancellationToken = default)
    {
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser is not null)
        {
            throw new ConflictException($"Email '{request.Email}' is already registered.");
        }

        var empNoExists = await _unitOfWork.Teachers.AnyAsync(
            t => t.EmployeeNumber == request.EmployeeNumber,
            cancellationToken);
        if (empNoExists)
        {
            throw new ConflictException($"Employee number '{request.EmployeeNumber}' already exists.");
        }

        var user = new ApplicationUser
        {
            FullName = request.FullName,
            Email = request.Email,
            UserName = request.Email,
            IsActive = true
        };

        var createResult = await _userManager.CreateAsync(user, request.Password);
        if (!createResult.Succeeded)
        {
            var errors = createResult.Errors.Select(e => e.Description);
            throw new BadRequestException("Failed to create teacher user account.", errors);
        }

        await _userManager.AddToRoleAsync(user, UserRole.Teacher.ToString());

        var teacher = new Teacher
        {
            ApplicationUserId = user.Id,
            EmployeeNumber = request.EmployeeNumber,
            Department = request.Department,
            Specialization = request.Specialization,
            HireDate = request.HireDate
        };

        await _unitOfWork.Teachers.AddAsync(teacher, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return GenerateJwtTokenResponse(user, UserRole.Teacher, teacher.Id);
    }

    private AuthResponseDto GenerateJwtTokenResponse(ApplicationUser user, UserRole roleDomain, Guid? profileId)
    {
        var expiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.CreateVersion7().ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Role, roleDomain.ToString()),
            new("domain", roleDomain.ToString())
        };

        if (profileId.HasValue)
        {
            claims.Add(new Claim("profileId", profileId.Value.ToString()));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var tokenDescriptor = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials);

        var accessToken = new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);

        return new AuthResponseDto
        {
            UserId = user.Id,
            ProfileId = profileId,
            FullName = user.FullName,
            Email = user.Email ?? string.Empty,
            RoleDomain = roleDomain,
            AccessToken = accessToken,
            ExpiresAt = expiresAt
        };
    }
}

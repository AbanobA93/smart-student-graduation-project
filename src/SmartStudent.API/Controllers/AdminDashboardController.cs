using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartStudent.Application.Common;
using SmartStudent.Application.DTOs;
using SmartStudent.Application.Interfaces;

namespace SmartStudent.API.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
public class AdminDashboardController : ControllerBase
{
    private readonly IAdminDashboardService _adminService;

    public AdminDashboardController(IAdminDashboardService adminService)
    {
        _adminService = adminService;
    }

    [HttpGet("users")]
    public async Task<ActionResult<ApiResponse<PaginatedResult<UserSummaryDto>>>> GetAllUsers(
        [FromQuery] PaginationParams pagination,
        CancellationToken cancellationToken)
    {
        var result = await _adminService.GetAllUsersAsync(pagination, cancellationToken);
        return Ok(ApiResponse<PaginatedResult<UserSummaryDto>>.Success(result));
    }

    [HttpPut("users/role")]
    public async Task<ActionResult<ApiResponse<string>>> ChangeUserRole(
        [FromBody] ChangeUserRoleDto dto,
        CancellationToken cancellationToken)
    {
        await _adminService.ChangeUserRoleAsync(dto, cancellationToken);
        return Ok(ApiResponse<string>.Success("RoleUpdated", "User role updated successfully."));
    }

    [HttpGet("statistics")]
    public async Task<ActionResult<ApiResponse<SystemStatisticsDto>>> GetSystemStatistics(CancellationToken cancellationToken)
    {
        var result = await _adminService.GetSystemStatisticsAsync(cancellationToken);
        return Ok(ApiResponse<SystemStatisticsDto>.Success(result));
    }
}

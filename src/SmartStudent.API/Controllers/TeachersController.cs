using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartStudent.Application.Common;
using SmartStudent.Application.DTOs;
using SmartStudent.Application.Interfaces;

namespace SmartStudent.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class TeachersController : ControllerBase
{
    private readonly ITeacherService _teacherService;

    public TeachersController(ITeacherService teacherService)
    {
        _teacherService = teacherService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PaginatedResult<TeacherDto>>>> GetAll(
        [FromQuery] PaginationParams pagination,
        CancellationToken cancellationToken)
    {
        var result = await _teacherService.GetPagedTeachersAsync(pagination, cancellationToken);
        return Ok(ApiResponse<PaginatedResult<TeacherDto>>.Success(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<TeacherDto>>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _teacherService.GetTeacherByIdAsync(id, cancellationToken);
        return Ok(ApiResponse<TeacherDto>.Success(result));
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<string>>> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _teacherService.DeleteTeacherAsync(id, cancellationToken);
        return Ok(ApiResponse<string>.Success("Deleted", "Teacher deleted successfully."));
    }
}

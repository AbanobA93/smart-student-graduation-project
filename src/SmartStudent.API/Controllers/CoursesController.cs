using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartStudent.Application.Common;
using SmartStudent.Application.DTOs;
using SmartStudent.Application.Interfaces;

namespace SmartStudent.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CoursesController : ControllerBase
{
    private readonly ICourseService _courseService;

    public CoursesController(ICourseService courseService)
    {
        _courseService = courseService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PaginatedResult<CourseDto>>>> GetAll(
        [FromQuery] PaginationParams pagination,
        CancellationToken cancellationToken)
    {
        var result = await _courseService.GetPagedCoursesAsync(pagination, cancellationToken);
        return Ok(ApiResponse<PaginatedResult<CourseDto>>.Success(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<CourseDto>>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _courseService.GetCourseByIdAsync(id, cancellationToken);
        return Ok(ApiResponse<CourseDto>.Success(result));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<CourseDto>>> Create(
        [FromBody] CreateCourseDto dto,
        CancellationToken cancellationToken)
    {
        var result = await _courseService.CreateCourseAsync(dto, cancellationToken);
        return StatusCode(201, ApiResponse<CourseDto>.Success(result, "Course created successfully.", 201));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<CourseDto>>> Update(
        Guid id,
        [FromBody] UpdateCourseDto dto,
        CancellationToken cancellationToken)
    {
        var result = await _courseService.UpdateCourseAsync(id, dto, cancellationToken);
        return Ok(ApiResponse<CourseDto>.Success(result, "Course updated successfully."));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<string>>> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _courseService.DeleteCourseAsync(id, cancellationToken);
        return Ok(ApiResponse<string>.Success("Deleted", "Course deleted successfully."));
    }

    [HttpPost("assign-teacher")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<CourseDto>>> AssignTeacher(
        [FromBody] AssignTeacherToCourseDto dto,
        CancellationToken cancellationToken)
    {
        var result = await _courseService.AssignTeacherAsync(dto, cancellationToken);
        return Ok(ApiResponse<CourseDto>.Success(result, "Teacher assigned to course successfully."));
    }
}

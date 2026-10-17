using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartStudent.API.Common;
using SmartStudent.Application.Common;
using SmartStudent.Application.DTOs;
using SmartStudent.Application.Interfaces;

namespace SmartStudent.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class GradesController : ControllerBase
{
    private readonly IGradeService _gradeService;

    public GradesController(IGradeService gradeService)
    {
        _gradeService = gradeService;
    }

    /// <summary>
    /// Teacher adds a grade for a student in their assigned course.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Teacher")]
    public async Task<ActionResult<ApiResponse<GradeDto>>> AddGrade(
        [FromBody] CreateGradeDto dto,
        CancellationToken cancellationToken)
    {
        var teacherProfileId = User.GetProfileId();
        var result = await _gradeService.AddGradeAsync(teacherProfileId, dto, cancellationToken);
        return StatusCode(201, ApiResponse<GradeDto>.Success(result, "Grade recorded successfully.", 201));
    }

    /// <summary>
    /// Teacher updates a grade in their assigned course.
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Teacher")]
    public async Task<ActionResult<ApiResponse<GradeDto>>> UpdateGrade(
        Guid id,
        [FromBody] UpdateGradeDto dto,
        CancellationToken cancellationToken)
    {
        var teacherProfileId = User.GetProfileId();
        var result = await _gradeService.UpdateGradeAsync(teacherProfileId, id, dto, cancellationToken);
        return Ok(ApiResponse<GradeDto>.Success(result, "Grade updated successfully."));
    }

    /// <summary>
    /// Teacher views all grades for a course they teach.
    /// </summary>
    [HttpGet("course/{courseId:guid}")]
    [Authorize(Roles = "Teacher")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<GradeDto>>>> GetCourseGrades(
        Guid courseId,
        CancellationToken cancellationToken)
    {
        var teacherProfileId = User.GetProfileId();
        var result = await _gradeService.GetCourseGradesForTeacherAsync(teacherProfileId, courseId, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<GradeDto>>.Success(result));
    }

    /// <summary>
    /// Student views ONLY their own grades (extracted securely from JWT claims).
    /// </summary>
    [HttpGet("my-grades")]
    [Authorize(Roles = "Student")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<GradeDto>>>> GetMyGrades(CancellationToken cancellationToken)
    {
        var studentProfileId = User.GetProfileId();
        var result = await _gradeService.GetMyGradesForStudentAsync(studentProfileId, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<GradeDto>>.Success(result));
    }
}

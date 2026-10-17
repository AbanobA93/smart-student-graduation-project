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
public class AttendanceController : ControllerBase
{
    private readonly IAttendanceService _attendanceService;

    public AttendanceController(IAttendanceService attendanceService)
    {
        _attendanceService = attendanceService;
    }

    /// <summary>
    /// Teacher marks daily attendance for a student in their assigned course.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Teacher")]
    public async Task<ActionResult<ApiResponse<AttendanceDto>>> MarkAttendance(
        [FromBody] MarkAttendanceDto dto,
        CancellationToken cancellationToken)
    {
        var teacherProfileId = User.GetProfileId();
        var result = await _attendanceService.MarkAttendanceAsync(teacherProfileId, dto, cancellationToken);
        return StatusCode(201, ApiResponse<AttendanceDto>.Success(result, "Attendance marked successfully.", 201));
    }

    /// <summary>
    /// Teacher views attendance records for a course they teach.
    /// </summary>
    [HttpGet("course/{courseId:guid}")]
    [Authorize(Roles = "Teacher")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<AttendanceDto>>>> GetCourseAttendance(
        Guid courseId,
        [FromQuery] DateOnly? date,
        CancellationToken cancellationToken)
    {
        var teacherProfileId = User.GetProfileId();
        var result = await _attendanceService.GetCourseAttendanceForTeacherAsync(teacherProfileId, courseId, date, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<AttendanceDto>>.Success(result));
    }

    /// <summary>
    /// Student views ONLY their own attendance history (extracted securely from JWT claims).
    /// </summary>
    [HttpGet("my-attendance")]
    [Authorize(Roles = "Student")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<AttendanceDto>>>> GetMyAttendance(CancellationToken cancellationToken)
    {
        var studentProfileId = User.GetProfileId();
        var result = await _attendanceService.GetMyAttendanceForStudentAsync(studentProfileId, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<AttendanceDto>>.Success(result));
    }
}

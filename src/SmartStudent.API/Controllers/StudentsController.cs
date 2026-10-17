using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartStudent.Application.Common;
using SmartStudent.Application.DTOs;
using SmartStudent.Application.Interfaces;

namespace SmartStudent.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class StudentsController : ControllerBase
{
    private readonly IStudentService _studentService;
    private readonly IFileStorageService _fileStorageService;

    public StudentsController(IStudentService studentService, IFileStorageService fileStorageService)
    {
        _studentService = studentService;
        _fileStorageService = fileStorageService;
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<ActionResult<ApiResponse<PaginatedResult<StudentDto>>>> GetAll(
        [FromQuery] PaginationParams pagination,
        CancellationToken cancellationToken)
    {
        var result = await _studentService.GetPagedStudentsAsync(pagination, cancellationToken);
        return Ok(ApiResponse<PaginatedResult<StudentDto>>.Success(result));
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<ActionResult<ApiResponse<StudentDto>>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _studentService.GetStudentByIdAsync(id, cancellationToken);
        return Ok(ApiResponse<StudentDto>.Success(result));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<StudentDto>>> Update(
        Guid id,
        [FromBody] UpdateStudentDto dto,
        CancellationToken cancellationToken)
    {
        var result = await _studentService.UpdateStudentAsync(id, dto, cancellationToken);
        return Ok(ApiResponse<StudentDto>.Success(result, "Student updated successfully."));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<string>>> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _studentService.DeleteStudentAsync(id, cancellationToken);
        return Ok(ApiResponse<string>.Success("Deleted", "Student deleted successfully."));
    }

    [HttpPost("assign-classroom")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<string>>> AssignClassRoom(
        [FromBody] AssignStudentClassDto dto,
        CancellationToken cancellationToken)
    {
        await _studentService.AssignToClassRoomAsync(dto, cancellationToken);
        return Ok(ApiResponse<string>.Success("Assigned", "Student assigned to classroom successfully."));
    }

    [HttpPost("enroll-course")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<EnrollmentDto>>> EnrollInCourse(
        [FromBody] EnrollStudentCourseDto dto,
        CancellationToken cancellationToken)
    {
        var result = await _studentService.EnrollInCourseAsync(dto, cancellationToken);
        return StatusCode(201, ApiResponse<EnrollmentDto>.Success(result, "Student enrolled in course successfully.", 201));
    }

    [HttpPost("{id:guid}/photo")]
    [Authorize(Roles = "Admin,Student")]
    public async Task<ActionResult<ApiResponse<string>>> UploadPhoto(
        Guid id,
        IFormFile file,
        CancellationToken cancellationToken)
    {
        await using var stream = file.OpenReadStream();
        var photoUrl = await _fileStorageService.SaveProfilePhotoAsync(stream, file.FileName, cancellationToken);
        var updatedUrl = await _studentService.UpdateProfilePhotoAsync(id, photoUrl, cancellationToken);
        return Ok(ApiResponse<string>.Success(updatedUrl, "Profile photo uploaded successfully."));
    }
}

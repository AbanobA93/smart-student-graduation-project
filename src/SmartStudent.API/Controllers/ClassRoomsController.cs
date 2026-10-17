using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartStudent.Application.Common;
using SmartStudent.Application.DTOs;
using SmartStudent.Application.Interfaces;

namespace SmartStudent.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ClassRoomsController : ControllerBase
{
    private readonly IClassRoomService _classRoomService;

    public ClassRoomsController(IClassRoomService classRoomService)
    {
        _classRoomService = classRoomService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<ClassRoomDto>>>> GetAll(CancellationToken cancellationToken)
    {
        var result = await _classRoomService.GetAllAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<ClassRoomDto>>.Success(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<ClassRoomDto>>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _classRoomService.GetByIdAsync(id, cancellationToken);
        return Ok(ApiResponse<ClassRoomDto>.Success(result));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<ClassRoomDto>>> Create(
        [FromBody] CreateClassRoomDto dto,
        CancellationToken cancellationToken)
    {
        var result = await _classRoomService.CreateAsync(dto, cancellationToken);
        return StatusCode(201, ApiResponse<ClassRoomDto>.Success(result, "Classroom created successfully.", 201));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<string>>> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _classRoomService.DeleteAsync(id, cancellationToken);
        return Ok(ApiResponse<string>.Success("Deleted", "Classroom deleted successfully."));
    }
}

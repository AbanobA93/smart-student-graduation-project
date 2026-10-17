using AutoMapper;
using SmartStudent.Application.DTOs;
using SmartStudent.Application.Interfaces;
using SmartStudent.Core.Entities;
using SmartStudent.Core.Exceptions;

namespace SmartStudent.Application.Services;

public class ClassRoomService : IClassRoomService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ClassRoomService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<ClassRoomDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var classRooms = await _unitOfWork.ClassRooms.GetAllAsync(
            asNoTracking: true,
            cancellationToken: cancellationToken);

        return _mapper.Map<IReadOnlyList<ClassRoomDto>>(classRooms);
    }

    public async Task<ClassRoomDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var classRoom = await _unitOfWork.ClassRooms.GetByIdAsync(
            id,
            asNoTracking: true,
            cancellationToken: cancellationToken)
            ?? throw new NotFoundException(nameof(ClassRoom), id);

        return _mapper.Map<ClassRoomDto>(classRoom);
    }

    public async Task<ClassRoomDto> CreateAsync(CreateClassRoomDto dto, CancellationToken cancellationToken = default)
    {
        var exists = await _unitOfWork.ClassRooms.AnyAsync(
            c => c.Name == dto.Name && c.AcademicYear == dto.AcademicYear,
            cancellationToken);

        if (exists)
        {
            throw new ConflictException($"Classroom '{dto.Name}' already exists for academic year {dto.AcademicYear}.");
        }

        var classRoom = _mapper.Map<ClassRoom>(dto);
        await _unitOfWork.ClassRooms.AddAsync(classRoom, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<ClassRoomDto>(classRoom);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var classRoom = await _unitOfWork.ClassRooms.GetByIdAsync(
            id,
            asNoTracking: false,
            cancellationToken: cancellationToken)
            ?? throw new NotFoundException(nameof(ClassRoom), id);

        _unitOfWork.ClassRooms.SoftDelete(classRoom);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

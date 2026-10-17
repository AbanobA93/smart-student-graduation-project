using AutoMapper;
using SmartStudent.Application.DTOs;
using SmartStudent.Core.Entities;

namespace SmartStudent.Application.Mappings;

/// <summary>
/// AutoMapper profile mapping domain entities to DTOs.
/// </summary>
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<ClassRoom, ClassRoomDto>();
        CreateMap<CreateClassRoomDto, ClassRoom>();

        CreateMap<Student, StudentDto>()
            .ForMember(d => d.UserId, opt => opt.MapFrom(s => s.ApplicationUserId))
            .ForMember(d => d.FullName, opt => opt.MapFrom(s => s.User != null ? s.User.FullName : string.Empty))
            .ForMember(d => d.Email, opt => opt.MapFrom(s => s.User != null ? s.User.Email ?? string.Empty : string.Empty))
            .ForMember(d => d.ProfilePhotoUrl, opt => opt.MapFrom(s => s.User != null ? s.User.ProfilePhotoUrl : null))
            .ForMember(d => d.ClassRoomName, opt => opt.MapFrom(s => s.ClassRoom != null ? s.ClassRoom.Name : null));

        CreateMap<Teacher, TeacherDto>()
            .ForMember(d => d.UserId, opt => opt.MapFrom(s => s.ApplicationUserId))
            .ForMember(d => d.FullName, opt => opt.MapFrom(s => s.User != null ? s.User.FullName : string.Empty))
            .ForMember(d => d.Email, opt => opt.MapFrom(s => s.User != null ? s.User.Email ?? string.Empty : string.Empty));

        CreateMap<Course, CourseDto>()
            .ForMember(d => d.TeacherName, opt => opt.MapFrom(s => s.Teacher != null && s.Teacher.User != null ? s.Teacher.User.FullName : null));
        CreateMap<CreateCourseDto, Course>();

        CreateMap<Enrollment, EnrollmentDto>()
            .ForMember(d => d.StudentNumber, opt => opt.MapFrom(s => s.Student != null ? s.Student.StudentNumber : string.Empty))
            .ForMember(d => d.StudentName, opt => opt.MapFrom(s => s.Student != null && s.Student.User != null ? s.Student.User.FullName : string.Empty))
            .ForMember(d => d.CourseCode, opt => opt.MapFrom(s => s.Course != null ? s.Course.CourseCode : string.Empty))
            .ForMember(d => d.CourseTitle, opt => opt.MapFrom(s => s.Course != null ? s.Course.Title : string.Empty));

        CreateMap<Grade, GradeDto>()
            .ForMember(d => d.StudentNumber, opt => opt.MapFrom(s => s.Student != null ? s.Student.StudentNumber : string.Empty))
            .ForMember(d => d.StudentName, opt => opt.MapFrom(s => s.Student != null && s.Student.User != null ? s.Student.User.FullName : string.Empty))
            .ForMember(d => d.CourseCode, opt => opt.MapFrom(s => s.Course != null ? s.Course.CourseCode : string.Empty))
            .ForMember(d => d.CourseTitle, opt => opt.MapFrom(s => s.Course != null ? s.Course.Title : string.Empty));

        CreateMap<Attendance, AttendanceDto>()
            .ForMember(d => d.StudentNumber, opt => opt.MapFrom(s => s.Student != null ? s.Student.StudentNumber : string.Empty))
            .ForMember(d => d.StudentName, opt => opt.MapFrom(s => s.Student != null && s.Student.User != null ? s.Student.User.FullName : string.Empty))
            .ForMember(d => d.CourseCode, opt => opt.MapFrom(s => s.Course != null ? s.Course.CourseCode : string.Empty))
            .ForMember(d => d.CourseTitle, opt => opt.MapFrom(s => s.Course != null ? s.Course.Title : string.Empty));
    }
}

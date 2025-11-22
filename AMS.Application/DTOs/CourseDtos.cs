namespace AMS.Application.DTOs;

public record CreateCourseDto(
    string Name,
    string Code,
    int Ects,
    Guid DepartmentId,
    Guid LecturerId
);

public record UpdateCourseDto(
    Guid Id,
    string Name,
    string Code,
    int Ects
);

public record CourseDto(
    Guid Id,
    string Name,
    string Code,
    int Ects,
    int PrerequisitesCount,
    string DepartmentName,
    string LecturerName
);

public record CourseDetailsDto(
    Guid Id,
    string Name,
    string Code,
    int Ects,
    string DepartmentName,
    string LecturerName,
    List<CourseDto> Prerequisites
);
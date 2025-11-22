namespace AMS.Application.DTOs;

public record EnrollStudentDto(
    Guid StudentId,
    Guid CourseId,
    string Semester
);

public record UpdateGradeDto(
    Guid EnrollmentId,
    double Grade
);

public record EnrollmentDto(
    Guid Id,
    string CourseName,
    string CourseCode,
    int Ects,
    string Semester,
    double? Grade
);
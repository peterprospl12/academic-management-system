namespace AMS.Application.DTOs;

public record TopProfessorDto(
    string FullName,
    string AcademicTitle,
    int TotalStudentsCount
);

public record CourseGpaDto(
    string CourseName,
    string CourseCode,
    double AverageGrade,
    int GradedStudentsCount
);

public record HardestPlanStudentDto(
    string FullName,
    string IndexNumber,
    int Semester,
    int CurrentEcts,
    int PrerequisitesEcts,
    int TotalDifficultyScore
);
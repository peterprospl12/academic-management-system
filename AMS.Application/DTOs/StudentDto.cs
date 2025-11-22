namespace AMS.Application.DTOs;

public record UpdateStudentDto(
    Guid Id,
    string FirstName,
    string LastName,
    int YearOfStudy,
    string Street,
    string City,
    string PostalCode
);

public record CreateStudentDto(
    string FirstName,
    string LastName,
    string Street,
    string City,
    string PostalCode,
    int YearOfStudy
);

public record StudentDto(
    Guid Id,
    string FirstName,
    string LastName,
    string UniversityIndex,
    int YearOfStudy,
    string Street,
    string City,
    string PostalCode
);
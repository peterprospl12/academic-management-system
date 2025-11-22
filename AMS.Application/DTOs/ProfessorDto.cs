using AMS.Domain.Enums;

namespace AMS.Application.DTOs;

public record CreateProfessorDto(
    string FirstName,
    string LastName,
    string Street,
    string City,
    string PostalCode,
    AcademicTitle AcademicTitle
);

public record UpdateProfessorDto(
    Guid Id,
    string FirstName,
    string LastName,
    AcademicTitle AcademicTitle,
    string Street,
    string City,
    string PostalCode
);

public record ProfessorDto(
    Guid Id,
    string FirstName,
    string LastName,
    AcademicTitle Title,
    string UniversityIndex,
    string Street,
    string City,
    string OfficeRoom
);
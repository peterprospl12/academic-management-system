namespace AMS.Application.DTOs;

public record CreateStudentDto(
    string FirstName,
    string LastName,
    string Street,
    string City,
    string PostalCode,
    int YearOfStudy
);

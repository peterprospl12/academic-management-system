namespace AMS.Application.DTOs;

public record CreateMasterStudentDto(
    string FirstName,
    string LastName,
    int YearOfStudy,
    string Street,
    string City,
    string PostalCode,
    string ThesisTopic,
    Guid? PromoterId
);

public record MasterStudentDto(
    Guid Id,
    string FirstName,
    string LastName,
    string Index,
    string ThesisTopic,
    string PromoterName
);

public record UpdateMasterStudentDto(
    Guid Id,
    string FirstName,
    string LastName,
    int YearOfStudy,
    string Street,
    string City,
    string PostalCode,
    string ThesisTopic,
    Guid? PromoterId
);
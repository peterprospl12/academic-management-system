namespace AMS.Application.DTOs;

public record UpdateDepartmentDto(
    Guid Id,
    string Name
);

public record DepartmentDto(
    Guid Id,
    string Name
);

public record CreateDepartmentDto(
    string Name
);
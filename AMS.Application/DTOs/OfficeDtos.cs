namespace AMS.Application.DTOs;

public record CreateOfficeDto(
    string Building,
    string RoomNumber
);

public record UpdateOfficeDto(
    Guid Id,
    string Building,
    string RoomNumber
);

public record OfficeDto(
    Guid Id,
    string Building,
    string RoomNumber,
    Guid? OccupantId
);
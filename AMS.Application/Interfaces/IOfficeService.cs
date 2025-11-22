using AMS.Application.Common.Models;
using AMS.Application.DTOs;

namespace AMS.Application.Interfaces;

public interface IOfficeService
{
    Task<Result<Guid>> CreateOfficeAsync(CreateOfficeDto dto, CancellationToken ct);
    Task<Result> UpdateOfficeAsync(UpdateOfficeDto dto, CancellationToken ct);
    Task<Result> DeleteOfficeAsync(Guid id, CancellationToken ct);

    Task<Result<List<OfficeDto>>> GetAllOfficesAsync(CancellationToken ct);

    Task<Result> AssignProfessorToOfficeAsync(Guid officeId, Guid professorId, CancellationToken ct);
}
using AMS.Application.Common.Models;
using AMS.Application.DTOs;

namespace AMS.Application.Interfaces;

public interface IDepartmentService
{
    Task<Result<Guid>> CreateDepartmentAsync(CreateDepartmentDto dto, CancellationToken cancellationToken);
    Task<Result> DeleteDepartmentAsync(Guid id, CancellationToken cancellationToken);
    Task<Result> UpdateDepartmentAsync(UpdateDepartmentDto dto, CancellationToken cancellationToken);
    Task<Result<DepartmentDto>> GetDepartmentByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<Result<List<DepartmentDto>>> GetAllDepartmentsAsync(CancellationToken cancellationToken);
}
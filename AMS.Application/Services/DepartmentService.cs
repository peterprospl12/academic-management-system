using AMS.Application.Common.Interfaces;
using AMS.Application.Common.Models;
using AMS.Application.DTOs;
using AMS.Application.Interfaces;
using AMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AMS.Application.Services;

public class DepartmentService(IApplicationDbContext context) : IDepartmentService
{
    public async Task<Result> UpdateDepartmentAsync(UpdateDepartmentDto dto, CancellationToken cancellationToken)
    {
        var department = await context.Departments
            .FirstOrDefaultAsync(d => d.Id == dto.Id, cancellationToken)
            .ConfigureAwait(false);

        if (department == null)
            return Result.Failure($"Department with ID {dto.Id} does not exist.");

        var normalizedName = dto.Name.Trim();

        var nameTaken = await context.Departments
            .AnyAsync(d => d.Id != dto.Id && d.Name == normalizedName, cancellationToken)
            .ConfigureAwait(false);

        if (nameTaken)
            return Result.Failure($"Department name \"{normalizedName}\" is already taken.");

        department.Name = normalizedName;
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Result.Success();
    }

    public async Task<Result> DeleteDepartmentAsync(Guid departmentId, CancellationToken cancellationToken)
    {
        var department = await context.Departments
            .FirstOrDefaultAsync(d => d.Id == departmentId, cancellationToken)
            .ConfigureAwait(false);

        if (department == null)
            return Result.Failure($"Department with ID {departmentId} does not exist.");

        context.Departments.Remove(department);
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Result.Success();
    }

    public async Task<Result<DepartmentDto>> GetDepartmentByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var department = await context.Departments
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken)
            .ConfigureAwait(false);

        if (department == null)
            return Result<DepartmentDto>.Failure($"Department with ID {id} not found.");

        return Result<DepartmentDto>.Success(new DepartmentDto(department.Id, department.Name));
    }

    public async Task<Result<List<DepartmentDto>>> GetAllDepartmentsAsync(CancellationToken cancellationToken)
    {
        var departments = await context.Departments
            .AsNoTracking()
            .Select(d => new DepartmentDto(d.Id, d.Name))
            .OrderBy(d => d.Name)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return Result<List<DepartmentDto>>.Success(departments);
    }

    public async Task<Result<Guid>> CreateDepartmentAsync(CreateDepartmentDto dto, CancellationToken cancellationToken)
    {
        var normalizedName = dto.Name.Trim();

        var exists = await context.Departments
            .AnyAsync(d => d.Name == normalizedName, cancellationToken).ConfigureAwait(false);

        if (exists)
            return Result<Guid>.Failure($"Department \"{normalizedName}\" already exists.");

        var department = new Department { Name = normalizedName };

        await context.Departments.AddAsync(department, cancellationToken).ConfigureAwait(false);
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Result<Guid>.Success(department.Id);
    }
}
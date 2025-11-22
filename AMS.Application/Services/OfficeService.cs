using AMS.Application.Common.Interfaces;
using AMS.Application.Common.Models;
using AMS.Application.DTOs;
using AMS.Application.Interfaces;
using AMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AMS.Application.Services;

public class OfficeService(IApplicationDbContext context) : IOfficeService
{
    public async Task<Result<Guid>> CreateOfficeAsync(CreateOfficeDto dto, CancellationToken ct)
    {
        var exists = await context.Offices
            .AnyAsync(o => o.Building == dto.Building && o.RoomNumber == dto.RoomNumber, ct)
            .ConfigureAwait(false);

        if (exists)
            return Result<Guid>.Failure($"Office {dto.Building} {dto.RoomNumber} already exists.");

        var office = new Office
        {
            Building = dto.Building,
            RoomNumber = dto.RoomNumber
        };

        await context.Offices.AddAsync(office, ct).ConfigureAwait(false);
        await context.SaveChangesAsync(ct).ConfigureAwait(false);

        return Result<Guid>.Success(office.Id);
    }

    public async Task<Result> UpdateOfficeAsync(UpdateOfficeDto dto, CancellationToken ct)
    {
        var office = await context.Offices.FirstOrDefaultAsync(o => o.Id == dto.Id, ct).ConfigureAwait(false);
        if (office == null) return Result.Failure("Office not found.");

        office.Building = dto.Building;
        office.RoomNumber = dto.RoomNumber;

        await context.SaveChangesAsync(ct).ConfigureAwait(false);
        return Result.Success();
    }

    public async Task<Result> DeleteOfficeAsync(Guid id, CancellationToken ct)
    {
        var office = await context.Offices.FirstOrDefaultAsync(o => o.Id == id, ct).ConfigureAwait(false);
        if (office == null) return Result.Failure("Office not found.");

        context.Offices.Remove(office);
        await context.SaveChangesAsync(ct).ConfigureAwait(false);
        return Result.Success();
    }

    public async Task<Result<List<OfficeDto>>> GetAllOfficesAsync(CancellationToken ct)
    {
        var offices = await context.Offices
            .AsNoTracking()
            .Include(o => o.Professor)
            .Select(o => new OfficeDto(
                o.Id,
                o.Building,
                o.RoomNumber,
                o.Professor != null ? $"{o.Professor.AcademicTitle} {o.Professor.LastName}" : "Empty"
            ))
            .ToListAsync(ct)
            .ConfigureAwait(false);

        return Result<List<OfficeDto>>.Success(offices);
    }

    public async Task<Result> AssignProfessorToOfficeAsync(Guid officeId, Guid professorId, CancellationToken ct)
    {
        var office = await context.Offices
            .Include(o => o.Professor)
            .FirstOrDefaultAsync(o => o.Id == officeId, ct)
            .ConfigureAwait(false);

        if (office == null) return Result.Failure("Office not found.");

        if (office.Professor != null && office.Professor.Id != professorId)
            return Result.Failure($"Office is already occupied by {office.Professor.LastName}.");

        var existingOffice = await context.Offices
            .FirstOrDefaultAsync(o => o.ProfessorId == professorId && o.Id != officeId, ct)
            .ConfigureAwait(false);

        if (existingOffice != null)
            existingOffice.ProfessorId = null;

        office.ProfessorId = professorId;

        await context.SaveChangesAsync(ct).ConfigureAwait(false);
        return Result.Success();
    }
}
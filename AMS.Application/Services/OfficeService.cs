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
                o.RoomNumber,
                o.Building,
                o.ProfessorId
            ))
            .ToListAsync(ct)
            .ConfigureAwait(false);

        return Result<List<OfficeDto>>.Success(offices);
    }

    public async Task<Result> AssignProfessorToOfficeAsync(Guid officeId, Guid professorId, CancellationToken ct)
    {
        var targetOffice = await context.Offices
            .Include(o => o.Professor)
            .FirstOrDefaultAsync(o => o.Id == officeId, ct)
            .ConfigureAwait(false);

        if (targetOffice == null)
            return Result.Failure("Target office not found.");

        if (targetOffice.ProfessorId.HasValue && targetOffice.ProfessorId != professorId)
            return Result.Failure($"Office is already occupied by {targetOffice.Professor?.LastName}.");

        var professor = await context.Professors
            .Include(p => p.Office)
            .FirstOrDefaultAsync(p => p.Id == professorId, ct)
            .ConfigureAwait(false);

        if (professor == null)
            return Result.Failure("Professor not found.");

        if (professor.Office != null && professor.Office.Id != officeId)
        {
            professor.Office.ProfessorId = null;
            professor.Office = null;
        }

        var oldOffice = await context.Offices
            .FirstOrDefaultAsync(o => o.ProfessorId == professorId && o.Id != officeId, ct)
            .ConfigureAwait(false);

        if (oldOffice != null) oldOffice.ProfessorId = null;

        targetOffice.ProfessorId = professorId;

        targetOffice.Professor = professor;
        professor.Office = targetOffice;

        await context.SaveChangesAsync(ct).ConfigureAwait(false);
        return Result.Success();
    }
}
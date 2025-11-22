using AMS.Application.Common.Interfaces;
using AMS.Application.Common.Models;
using AMS.Application.DTOs;
using AMS.Application.Interfaces;
using AMS.Domain.Entities;
using AMS.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace AMS.Application.Services;

public sealed class ProfessorService(IApplicationDbContext context) : IProfessorService
{
    public async Task<Result<Guid>> CreateProfessorAsync(CreateProfessorDto dto, CancellationToken cancellationToken)
    {
        await using var transaction = await context.Database
            .BeginTransactionAsync(cancellationToken)
            .ConfigureAwait(false);

        try
        {
            var counter = await context.SequenceCounters
                .FirstOrDefaultAsync(s => s.Prefix == "P", cancellationToken)
                .ConfigureAwait(false);

            if (counter == null)
            {
                counter = new SequenceCounter { Prefix = "P", CurrentValue = 0 };
                await context.SequenceCounters
                    .AddAsync(counter, cancellationToken)
                    .ConfigureAwait(false);
            }

            counter.CurrentValue++;
            var newIndex = $"{counter.Prefix}{counter.CurrentValue}";

            var professor = new Professor
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                AcademicTitle = dto.AcademicTitle,
                UniversityIndex = newIndex,
                Address = new Address(dto.Street, dto.City, dto.PostalCode)
            };

            await context.Professors.AddAsync(professor, cancellationToken).ConfigureAwait(false);
            await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);

            return Result<Guid>.Success(professor.Id);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
            return Result<Guid>.Failure($"Error creating professor: {ex.Message}");
        }
    }

    public async Task<Result> DeleteProfessorAsync(Guid professorId, CancellationToken cancellationToken)
    {
        await using var transaction = await context.Database
            .BeginTransactionAsync(cancellationToken)
            .ConfigureAwait(false);

        try
        {
            var professor = await context.Professors
                .Include(p => p.Office)
                .FirstOrDefaultAsync(p => p.Id == professorId, cancellationToken)
                .ConfigureAwait(false);

            if (professor == null) return Result.Failure("Professor not found.");

            var counter = await context.SequenceCounters
                .FirstOrDefaultAsync(s => s.Prefix == "P", cancellationToken)
                .ConfigureAwait(false);

            if (counter != null)
                if (int.TryParse(professor.UniversityIndex.Substring(1), out var profNumber))
                    if (counter.CurrentValue == profNumber)
                        counter.CurrentValue--;

            context.Professors.Remove(professor);
            await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);

            return Result.Success();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
            return Result.Failure($"Error deleting professor: {ex.Message}");
        }
    }

    public async Task<Result<ProfessorDto>> GetProfessorByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var professor = await context.Professors
            .AsNoTracking()
            .Include(p => p.Office)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken)
            .ConfigureAwait(false);

        if (professor == null) return Result<ProfessorDto>.Failure("Professor not found.");

        var dto = new ProfessorDto(
            professor.Id,
            professor.FirstName,
            professor.LastName,
            professor.AcademicTitle,
            professor.UniversityIndex,
            professor.Address.Street,
            professor.Address.City,
            professor.Office?.RoomNumber ?? "None"
        );

        return Result<ProfessorDto>.Success(dto);
    }

    public async Task<Result<List<ProfessorDto>>> GetAllProfessorsAsync(CancellationToken cancellationToken)
    {
        var professors = await context.Professors
            .AsNoTracking()
            .Include(p => p.Office)
            .Select(p => new ProfessorDto(
                p.Id,
                p.FirstName,
                p.LastName,
                p.AcademicTitle,
                p.UniversityIndex,
                p.Address.Street,
                p.Address.City,
                p.Office != null ? p.Office.RoomNumber : "Brak"
            ))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return Result<List<ProfessorDto>>.Success(professors);
    }

    public async Task<Result> UpdateProfessorAsync(UpdateProfessorDto dto, CancellationToken cancellationToken)
    {
        var professor = await context.Professors
            .FirstOrDefaultAsync(p => p.Id == dto.Id, cancellationToken)
            .ConfigureAwait(false);

        if (professor == null) return Result.Failure("Professor not found.");

        professor.FirstName = dto.FirstName;
        professor.LastName = dto.LastName;
        professor.AcademicTitle = dto.AcademicTitle;
        professor.Address = new Address(dto.Street, dto.City, dto.PostalCode);

        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result.Success();
    }
}
using AMS.Application.Common.Interfaces;
using AMS.Application.Common.Models;
using AMS.Application.DTOs;
using AMS.Application.Interfaces;
using AMS.Domain.Entities;
using AMS.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace AMS.Application.Services;

public class MasterStudentService(IApplicationDbContext context) : IMasterStudentService
{
    public async Task<Result<Guid>> CreateMasterStudentAsync(CreateMasterStudentDto dto,
        CancellationToken cancellationToken)
    {
        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken)
            .ConfigureAwait(false);

        try
        {
            var counter = await context.SequenceCounters
                .FirstOrDefaultAsync(s => s.Prefix == "S", cancellationToken)
                .ConfigureAwait(false);

            if (counter == null)
            {
                counter = new SequenceCounter { Prefix = "S", CurrentValue = 0 };
                await context.SequenceCounters
                    .AddAsync(counter, cancellationToken)
                    .ConfigureAwait(false);
            }

            counter.CurrentValue++;
            var newIndex = $"{counter.Prefix}{counter.CurrentValue}";

            Professor? promoter = null;
            if (dto.PromoterId.HasValue)
            {
                promoter = await context.Professors.FindAsync(new object[] { dto.PromoterId.Value }, cancellationToken)
                    .ConfigureAwait(false);
                if (promoter == null) return Result<Guid>.Failure("Promoter not found.");
            }

            var masterStudent = new MasterStudent
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                YearOfStudy = dto.YearOfStudy,
                UniversityIndex = newIndex,
                Address = new Address(dto.Street, dto.City, dto.PostalCode),
                ThesisTopic = dto.ThesisTopic,
                PromoterId = dto.PromoterId
            };

            await context.MasterStudents
                .AddAsync(masterStudent, cancellationToken)
                .ConfigureAwait(false);

            await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);

            return Result<Guid>.Success(masterStudent.Id);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
            return Result<Guid>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<MasterStudentDto>> GetMasterStudentByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var student = await context.MasterStudents
            .AsNoTracking()
            .Include(m => m.Promoter)
            .FirstOrDefaultAsync(m => m.Id == id, cancellationToken)
            .ConfigureAwait(false);

        if (student == null) return Result<MasterStudentDto>.Failure("Master student not found.");

        var dto = new MasterStudentDto(
            student.Id,
            student.FirstName,
            student.LastName,
            student.UniversityIndex,
            student.ThesisTopic,
            student.Promoter != null ? $"{student.Promoter.FirstName} {student.Promoter.LastName}" : "Brak"
        );

        return Result<MasterStudentDto>.Success(dto);
    }

    public async Task<Result> UpdateMasterStudentAsync(UpdateMasterStudentDto dto, CancellationToken cancellationToken)
    {
        var student = await context.MasterStudents
            .FirstOrDefaultAsync(m => m.Id == dto.Id, cancellationToken)
            .ConfigureAwait(false);

        if (student == null)
            return Result.Failure("Master student not found.");

        student.FirstName = dto.FirstName;
        student.LastName = dto.LastName;
        student.YearOfStudy = dto.YearOfStudy;
        student.Address = new Address(dto.Street, dto.City, dto.PostalCode);

        student.ThesisTopic = dto.ThesisTopic;

        if (dto.PromoterId.HasValue)
        {
            var promoterExists = await context.Professors
                .AnyAsync(p => p.Id == dto.PromoterId.Value, cancellationToken)
                .ConfigureAwait(false);

            if (!promoterExists)
                return Result.Failure("Promoter not found.");

            student.PromoterId = dto.PromoterId;
        }
        else
        {
            student.PromoterId = null;
        }

        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result.Success();
    }

    public async Task<Result> DeleteMasterStudentAsync(Guid id, CancellationToken cancellationToken)
    {
        await using var transaction = await context.Database
            .BeginTransactionAsync(cancellationToken)
            .ConfigureAwait(false);

        try
        {
            var student = await context.MasterStudents
                .FirstOrDefaultAsync(m => m.Id == id, cancellationToken)
                .ConfigureAwait(false);

            if (student == null)
                return Result.Failure("Master student not found.");

            var counter = await context.SequenceCounters
                .FirstOrDefaultAsync(s => s.Prefix == "S", cancellationToken)
                .ConfigureAwait(false);

            if (counter != null)
                if (int.TryParse(student.UniversityIndex.Substring(1), out var indexNumber))
                    if (counter.CurrentValue == indexNumber)
                        counter.CurrentValue--;

            context.MasterStudents.Remove(student);
            await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);

            return Result.Success();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
            return Result.Failure($"Error deleting master student: {ex.Message}");
        }
    }
}
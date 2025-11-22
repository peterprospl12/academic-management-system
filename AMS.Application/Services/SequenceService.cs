using AMS.Application.Common.Interfaces;
using AMS.Application.Common.Models;
using AMS.Application.DTOs;
using AMS.Application.Interfaces;
using AMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AMS.Application.Services;

public class SequenceService(IApplicationDbContext context) : ISequenceService
{
    public async Task<Result> CreateSequenceAsync(CreateSequenceDto dto, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(dto.Prefix)) return Result.Failure("Prefix cannot be empty.");

        var exists = await context.SequenceCounters
            .AnyAsync(s => s.Prefix == dto.Prefix, cancellationToken)
            .ConfigureAwait(false);

        if (exists) return Result.Failure($"Sequence for prefix '{dto.Prefix}' already exists.");

        var sequence = new SequenceCounter
        {
            Prefix = dto.Prefix.ToUpper(),
            CurrentValue = dto.InitialValue
        };

        await context.SequenceCounters
            .AddAsync(sequence, cancellationToken)
            .ConfigureAwait(false);
        await context.SaveChangesAsync(cancellationToken)
            .ConfigureAwait(false);

        return Result.Success();
    }

    public async Task<Result<List<SequenceDto>>> GetAllSequencesAsync(CancellationToken cancellationToken)
    {
        var sequences = await context.SequenceCounters
            .AsNoTracking()
            .Select(s => new SequenceDto(s.Prefix, s.CurrentValue))
            .OrderBy(s => s.Prefix)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return Result<List<SequenceDto>>.Success(sequences);
    }

    public async Task<Result> DeleteSequenceAsync(string prefix, CancellationToken cancellationToken)
    {
        var sequence = await context.SequenceCounters
            .FirstOrDefaultAsync(s => s.Prefix == prefix.ToUpper(), cancellationToken)
            .ConfigureAwait(false);

        if (sequence == null)
            return Result.Failure($"Sequence with prefix {prefix} not found.");

        context.SequenceCounters.Remove(sequence);
        await context.SaveChangesAsync(cancellationToken)
            .ConfigureAwait(false);

        return Result.Success();
    }
}
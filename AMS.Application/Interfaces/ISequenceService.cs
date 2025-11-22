using AMS.Application.Common.Models;
using AMS.Application.DTOs;

namespace AMS.Application.Interfaces;

public interface ISequenceService
{
    Task<Result> CreateSequenceAsync(CreateSequenceDto dto, CancellationToken cancellationToken);
    Task<Result<List<SequenceDto>>> GetAllSequencesAsync(CancellationToken cancellationToken);
    Task<Result> DeleteSequenceAsync(string prefix, CancellationToken cancellationToken);
}
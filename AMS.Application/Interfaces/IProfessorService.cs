using AMS.Application.Common.Models;
using AMS.Application.DTOs;

namespace AMS.Application.Interfaces;

public interface IProfessorService
{
    Task<Result<Guid>> CreateProfessorAsync(CreateProfessorDto dto, CancellationToken cancellationToken);
    Task<Result> DeleteProfessorAsync(Guid professorId, CancellationToken cancellationToken);
    Task<Result<ProfessorDto>> GetProfessorByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<Result<List<ProfessorDto>>> GetAllProfessorsAsync(CancellationToken cancellationToken);
    Task<Result> UpdateProfessorAsync(UpdateProfessorDto dto, CancellationToken cancellationToken);
}
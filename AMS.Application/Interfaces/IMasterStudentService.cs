using AMS.Application.Common.Models;
using AMS.Application.DTOs;

namespace AMS.Application.Interfaces;

public interface IMasterStudentService
{
    Task<Result<Guid>> CreateMasterStudentAsync(CreateMasterStudentDto dto, CancellationToken cancellationToken);
    Task<Result<MasterStudentDto>> GetMasterStudentByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<Result> UpdateMasterStudentAsync(UpdateMasterStudentDto dto, CancellationToken cancellationToken);
    Task<Result> DeleteMasterStudentAsync(Guid id, CancellationToken cancellationToken);
}
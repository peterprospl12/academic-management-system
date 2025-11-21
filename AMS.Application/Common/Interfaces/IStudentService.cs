using AMS.Application.DTOs;

namespace AMS.Application.Common.Interfaces;

public interface IStudentService
{
    Task<Guid> CreateStudentAsync(CreateStudentDto dto, CancellationToken cancellationToken);
}
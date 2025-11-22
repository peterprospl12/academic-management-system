using AMS.Application.Common.Models;
using AMS.Application.DTOs;

namespace AMS.Application.Interfaces;

public interface IStudentService
{
    Task<Result<Guid>> CreateStudentAsync(CreateStudentDto dto, CancellationToken cancellationToken);
    Task<Result> DeleteStudentAsync(Guid studentId, CancellationToken cancellationToken);
    Task<Result<StudentDto>> GetStudentByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<Result<List<StudentDto>>> GetAllStudentsAsync(CancellationToken cancellationToken);
    Task<Result> UpdateStudentAsync(UpdateStudentDto dto, CancellationToken cancellationToken);
}
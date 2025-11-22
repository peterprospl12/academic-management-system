using AMS.Application.Common.Models;
using AMS.Application.DTOs;

namespace AMS.Application.Interfaces;

public interface IEnrollmentService
{
    Task<Result<Guid>> EnrollStudentAsync(EnrollStudentDto dto, CancellationToken cancellationToken);

    Task<Result> GradeStudentAsync(UpdateGradeDto dto, CancellationToken cancellationToken);

    Task<Result> UnenrollStudentAsync(Guid enrollmentId, CancellationToken cancellationToken);

    Task<Result<List<EnrollmentDto>>> GetStudentEnrollmentsAsync(Guid studentId, CancellationToken cancellationToken);
}
using AMS.Application.Common.Models;
using AMS.Application.DTOs;

namespace AMS.Application.Interfaces;

public interface IReportsService
{
    Task<Result<TopProfessorDto>> GetMostPopularProfessorAsync(CancellationToken ct);

    Task<Result<List<CourseGpaDto>>> GetCoursesGpaByDepartmentAsync(Guid departmentId, CancellationToken ct);

    Task<Result<HardestPlanStudentDto>> GetStudentWithHardestPlanAsync(CancellationToken ct);
}
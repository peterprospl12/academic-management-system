using AMS.Application.Common.Models;
using AMS.Application.DTOs;

namespace AMS.Application.Interfaces;

public interface ICourseService
{
    Task<Result<Guid>> CreateCourseAsync(CreateCourseDto dto, CancellationToken cancellationToken);
    Task<Result> UpdateCourseAsync(UpdateCourseDto dto, CancellationToken cancellationToken);
    Task<Result> DeleteCourseAsync(Guid id, CancellationToken cancellationToken);

    Task<Result<List<CourseDto>>> GetAllCoursesAsync(CancellationToken cancellationToken);
    Task<Result<CourseDetailsDto>> GetCourseDetailsAsync(Guid id, CancellationToken cancellationToken);

    Task<Result> AddPrerequisiteAsync(Guid courseId, Guid prerequisiteId, CancellationToken cancellationToken);
    Task<Result> RemovePrerequisiteAsync(Guid courseId, Guid prerequisiteId, CancellationToken cancellationToken);
}
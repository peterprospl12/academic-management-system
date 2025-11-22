using AMS.Application.Common.Interfaces;
using AMS.Application.Common.Models;
using AMS.Application.DTOs;
using AMS.Application.Interfaces;
using AMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AMS.Application.Services;

public sealed class CourseService(IApplicationDbContext context) : ICourseService
{
    public async Task<Result<Guid>> CreateCourseAsync(CreateCourseDto dto, CancellationToken cancellationToken)
    {
        var departmentExists = await context.Departments
            .AnyAsync(d => d.Id == dto.DepartmentId, cancellationToken)
            .ConfigureAwait(false);

        if (!departmentExists)
            return Result<Guid>.Failure("Department not found.");

        var lecturerExists = await context.Professors
            .AnyAsync(p => p.Id == dto.LecturerId, cancellationToken)
            .ConfigureAwait(false);

        if (!lecturerExists)
            return Result<Guid>.Failure("Lecturer not found.");

        var exists = await context.Courses.AnyAsync(c => c.CourseCode == dto.Code, cancellationToken)
            .ConfigureAwait(false);
        if (exists) return Result<Guid>.Failure($"Course with code '{dto.Code}' already exists.");

        var course = new Course
        {
            Name = dto.Name,
            CourseCode = dto.Code,
            Ects = dto.Ects,
            DepartmentId = dto.DepartmentId,
            LecturerId = dto.LecturerId
        };

        await context.Courses.AddAsync(course, cancellationToken).ConfigureAwait(false);
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Result<Guid>.Success(course.Id);
    }

    public async Task<Result> UpdateCourseAsync(UpdateCourseDto dto, CancellationToken cancellationToken)
    {
        var course = await context.Courses.FirstOrDefaultAsync(c => c.Id == dto.Id, cancellationToken)
            .ConfigureAwait(false);
        if (course == null) return Result.Failure("Course not found.");

        var codeTaken =
            await context.Courses.AnyAsync(c => c.CourseCode == dto.Code && c.Id != dto.Id, cancellationToken);
        if (codeTaken) return Result.Failure($"Code '{dto.Code}' is already taken.");

        course.Name = dto.Name;
        course.CourseCode = dto.Code;
        course.Ects = dto.Ects;

        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result.Success();
    }

    public async Task<Result> DeleteCourseAsync(Guid id, CancellationToken cancellationToken)
    {
        var course = await context.Courses
            .Include(c => c.Enrollments)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken)
            .ConfigureAwait(false);

        if (course == null) return Result.Failure("Course not found.");

        if (course.Enrollments.Count != 0)
            return Result.Failure("Cannot delete course with active enrollments.");

        context.Courses.Remove(course);
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result.Success();
    }

    public async Task<Result<List<CourseDto>>> GetAllCoursesAsync(CancellationToken cancellationToken)
    {
        var courses = await context.Courses
            .AsNoTracking()
            .Select(c => new CourseDto(
                c.Id,
                c.Name,
                c.CourseCode,
                c.Ects,
                c.Prerequisites.Count,
                c.Department.Name,
                GetProfessorFullName(c.Lecturer)
            ))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return Result<List<CourseDto>>.Success(courses);
    }

    public async Task<Result<CourseDetailsDto>> GetCourseDetailsAsync(Guid id, CancellationToken cancellationToken)
    {
        var course = await context.Courses
            .AsNoTracking()
            .Include(c => c.Prerequisites)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken)
            .ConfigureAwait(false);

        if (course == null) return Result<CourseDetailsDto>.Failure("Course not found.");

        var details = new CourseDetailsDto(
            course.Id,
            course.Name,
            course.CourseCode,
            course.Ects,
            course.Department.Name,
            GetProfessorFullName(course.Lecturer),
            course.Prerequisites.Select(p => new CourseDto(
                p.Id, p.Name, p.CourseCode, p.Ects, 0, p.Department.Name, GetProfessorFullName(p.Lecturer)
            )).ToList()
        );

        return Result<CourseDetailsDto>.Success(details);
    }


    public async Task<Result> AddPrerequisiteAsync(Guid courseId, Guid prerequisiteId,
        CancellationToken cancellationToken)
    {
        if (courseId == prerequisiteId)
            return Result.Failure("A course cannot be a prerequisite for itself.");

        var course = await context.Courses
            .Include(c => c.Prerequisites)
            .FirstOrDefaultAsync(c => c.Id == courseId, cancellationToken)
            .ConfigureAwait(false);

        var prereq = await context.Courses
            .FirstOrDefaultAsync(c => c.Id == prerequisiteId, cancellationToken)
            .ConfigureAwait(false);

        if (course == null || prereq == null) return Result.Failure("Course or Prerequisite not found.");

        if (course.Prerequisites.Any(p => p.Id == prerequisiteId))
            return Result.Failure("This prerequisite is already added.");

        course.Prerequisites.Add(prereq);

        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result.Success();
    }

    public async Task<Result> RemovePrerequisiteAsync(Guid courseId, Guid prerequisiteId,
        CancellationToken cancellationToken)
    {
        var course = await context.Courses
            .Include(c => c.Prerequisites)
            .FirstOrDefaultAsync(c => c.Id == courseId, cancellationToken)
            .ConfigureAwait(false);

        if (course == null) return Result.Failure("Course not found.");

        var prereq = course.Prerequisites.FirstOrDefault(p => p.Id == prerequisiteId);
        if (prereq == null) return Result.Failure("Prerequisite not found in this course.");

        course.Prerequisites.Remove(prereq);

        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result.Success();
    }

    private string GetProfessorFullName(Professor professor)
    {
        return $"{professor.AcademicTitle.ToString()} {professor.FirstName} {professor.LastName}";
    }
}
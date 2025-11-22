using AMS.Application.Common.Interfaces;
using AMS.Application.Common.Models;
using AMS.Application.DTOs;
using AMS.Application.Interfaces;
using AMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AMS.Application.Services;

public sealed class EnrollmentService(IApplicationDbContext context) : IEnrollmentService
{
    public async Task<Result<Guid>> EnrollStudentAsync(EnrollStudentDto dto, CancellationToken cancellationToken)
    {
        var studentExists = await context.Students.AnyAsync(s => s.Id == dto.StudentId, cancellationToken)
            .ConfigureAwait(false);
        if (!studentExists) return Result<Guid>.Failure("Student not found.");

        var courseExists = await context.Courses.AnyAsync(c => c.Id == dto.CourseId, cancellationToken)
            .ConfigureAwait(false);
        if (!courseExists) return Result<Guid>.Failure("Course not found.");

        var alreadyEnrolled = await context.Enrollments
            .AnyAsync(e => e.StudentId == dto.StudentId && e.CourseId == dto.CourseId, cancellationToken)
            .ConfigureAwait(false);

        if (alreadyEnrolled) return Result<Guid>.Failure("Student is already enrolled in this course.");

        var enrollment = new Enrollment
        {
            StudentId = dto.StudentId,
            CourseId = dto.CourseId,
            Semester = dto.Semester,
            Grade = null
        };

        await context.Enrollments.AddAsync(enrollment, cancellationToken).ConfigureAwait(false);
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Result<Guid>.Success(enrollment.Id);
    }

    public async Task<Result> GradeStudentAsync(UpdateGradeDto dto, CancellationToken cancellationToken)
    {
        var enrollment = await context.Enrollments
            .FirstOrDefaultAsync(e => e.Id == dto.EnrollmentId, cancellationToken)
            .ConfigureAwait(false);

        if (enrollment == null) return Result.Failure("Enrollment not found.");

        if (dto.Grade < 2.0 || dto.Grade > 5.5)
            return Result.Failure("Invalid grade value.");

        enrollment.Grade = dto.Grade;

        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result.Success();
    }

    public async Task<Result> UnenrollStudentAsync(Guid enrollmentId, CancellationToken cancellationToken)
    {
        var enrollment = await context.Enrollments
            .FirstOrDefaultAsync(e => e.Id == enrollmentId, cancellationToken)
            .ConfigureAwait(false);

        if (enrollment == null) return Result.Failure("Enrollment not found.");

        if (enrollment.Grade.HasValue)
            return Result.Failure("Cannot unenroll student who has already been graded.");

        context.Enrollments.Remove(enrollment);
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result.Success();
    }

    public async Task<Result<List<EnrollmentDto>>> GetStudentEnrollmentsAsync(Guid studentId,
        CancellationToken cancellationToken)
    {
        var enrollments = await context.Enrollments
            .AsNoTracking()
            .Where(e => e.StudentId == studentId)
            .Include(e => e.Course)
            .Select(e => new EnrollmentDto(
                e.Id,
                e.Course!.Name,
                e.Course.CourseCode,
                e.Course.Ects,
                e.Semester,
                e.Grade
            ))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return Result<List<EnrollmentDto>>.Success(enrollments);
    }
}
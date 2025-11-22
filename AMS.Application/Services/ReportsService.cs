using AMS.Application.Common.Interfaces;
using AMS.Application.Common.Models;
using AMS.Application.DTOs;
using AMS.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AMS.Application.Services;

public class ReportsService(IApplicationDbContext context) : IReportsService
{
    public async Task<Result<TopProfessorDto>> GetMostPopularProfessorAsync(CancellationToken ct)
    {
        var topProf = await context.Professors
            .AsNoTracking()
            .Select(p => new
            {
                FullName = p.FirstName + " " + p.LastName,
                Title = p.AcademicTitle,

                TotalStudents = context.Courses
                    .Where(c => c.LecturerId == p.Id)
                    .SelectMany(c => c.Enrollments)
                    .Count()
            })
            .OrderByDescending(x => x.TotalStudents)
            .FirstOrDefaultAsync(ct)
            .ConfigureAwait(false);

        if (topProf == null)
            return Result<TopProfessorDto>.Failure("Brak danych o profesorach lub studentach.");

        return Result<TopProfessorDto>.Success(new TopProfessorDto(
            topProf.FullName,
            topProf.Title.ToString(),
            topProf.TotalStudents
        ));
    }

    public async Task<Result<List<CourseGpaDto>>> GetCoursesGpaByDepartmentAsync(Guid departmentId,
        CancellationToken ct)
    {
        var report = await context.Courses
            .AsNoTracking()
            .Where(c => c.DepartmentId == departmentId)
            .Select(c => new CourseGpaDto(
                c.Name,
                c.CourseCode,
                c.Enrollments.Where(e => e.Grade.HasValue).Average(e => e.Grade!.Value),
                c.Enrollments.Count(e => e.Grade.HasValue)
            ))
            .OrderByDescending(c => c.AverageGrade)
            .ToListAsync(ct)
            .ConfigureAwait(false);

        return Result<List<CourseGpaDto>>.Success(report);
    }

    public async Task<Result<HardestPlanStudentDto>> GetStudentWithHardestPlanAsync(CancellationToken ct)
    {
        var hardestWorker = await context.Students
            .AsNoTracking()
            .Select(s => new
            {
                FullName = s.FirstName + " " + s.LastName,
                s.UniversityIndex,
                s.YearOfStudy,

                CurrentEcts = s.Enrollments.Sum(e => e.Course!.Ects),

                PrerequisitesEcts = s.Enrollments
                    .SelectMany(e => e.Course!.Prerequisites)
                    .Select(p => p.Ects)
                    .Distinct()
                    .Sum()
            })
            .OrderByDescending(x => x.CurrentEcts + x.PrerequisitesEcts)
            .FirstOrDefaultAsync(ct)
            .ConfigureAwait(false);

        if (hardestWorker == null)
            return Result<HardestPlanStudentDto>.Failure("Brak studentów w bazie.");

        var totalScore = hardestWorker.CurrentEcts + hardestWorker.PrerequisitesEcts;

        return Result<HardestPlanStudentDto>.Success(new HardestPlanStudentDto(
            hardestWorker.FullName,
            hardestWorker.UniversityIndex,
            hardestWorker.YearOfStudy,
            hardestWorker.CurrentEcts,
            hardestWorker.PrerequisitesEcts,
            totalScore
        ));
    }
}
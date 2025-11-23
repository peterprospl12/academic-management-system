using AMS.Application.Common.Interfaces;
using AMS.Application.Common.Models;
using AMS.Application.DTOs;
using AMS.Application.Interfaces;
using AMS.Domain.Enums;
using Bogus;
using Microsoft.EntityFrameworkCore;

namespace AMS.Infrastructure.Persistence.Seeder;

public class DataSeeder(
    IApplicationDbContext context,
    IDepartmentService departmentService,
    IProfessorService professorService,
    IStudentService studentService,
    IMasterStudentService masterStudentService,
    ICourseService courseService,
    IEnrollmentService enrollmentService,
    IOfficeService officeService) : IDataSeederService
{
    private const int DefaultProfessorCount = 5;
    private const int DefaultStudentCount = 20;
    private const int DefaultCourseCount = 8;

    public async Task SeedAsync(CancellationToken cancellationToken)
    {
        if (await context.Students.AnyAsync(cancellationToken))
        {
            Console.WriteLine("Database is already seeded. Skipping");
            return;
        }

        await GenerateDataInternalAsync(
            DefaultStudentCount,
            DefaultProfessorCount,
            msg => Console.WriteLine(msg),
            cancellationToken,
            true);
    }

    public async Task<Result> GenerateDataAsync(int studentCount, int professorCount, Action<string> onProgress,
        CancellationToken ct)
    {
        try
        {
            await GenerateDataInternalAsync(studentCount, professorCount, onProgress, ct, false);
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Generation failed: {ex.Message}");
        }
    }

    private async Task GenerateDataInternalAsync(
        int studentCount,
        int professorCount,
        Action<string> log,
        CancellationToken ct,
        bool initialSeed)
    {
        log("--- Starting Data Generation ---");

        if (initialSeed) await SeedDepartmentsAsync(log, ct);

        var officeIds = await SeedOfficesAsync(professorCount + 2, log, ct);

        var professorIds = await SeedProfessorsAsync(professorCount, log, ct);

        await AssignOfficesToProfessorsAsync(officeIds, professorIds, log, ct);

        List<Guid> courseIds;
        if (initialSeed || !await context.Courses.AnyAsync(ct))
            courseIds = await SeedCoursesAsync(DefaultCourseCount, log, ct);
        else
            courseIds = await context.Courses.Select(c => c.Id).ToListAsync(ct);

        var studentIds = await SeedStudentsAsync(studentCount, log, ct);

        var masterCount = (int)(studentCount * 0.2);
        if (masterCount > 0)
        {
            var masterIds = await SeedMasterStudentsAsync(masterCount, professorIds, log, ct);
            studentIds.AddRange(masterIds);
        }

        await SeedEnrollmentsAsync(studentIds, courseIds, log, ct);

        log("\n--- Finished ---");
    }


    private async Task SeedDepartmentsAsync(Action<string> log, CancellationToken ct)
    {
        log("Seeding departments...");
        var departments = new[]
            { "Wydział Informatyki", "Wydział Matematyki", "Wydział Fizyki", "Wydział Zarządzania" };

        foreach (var name in departments)
            if (!await context.Departments.AnyAsync(d => d.Name == name, ct))
                await departmentService.CreateDepartmentAsync(new CreateDepartmentDto(name), ct);
    }

    private async Task<List<Guid>> SeedOfficesAsync(int count, Action<string> log, CancellationToken ct)
    {
        log($"Seeding {count} offices...");
        var createdIds = new List<Guid>();
        var faker = new Faker<CreateOfficeDto>("pl")
            .CustomInstantiator(f => new CreateOfficeDto(
                "Budynek " + f.PickRandom("A", "B", "C", "D"),
                f.Random.Number(100, 999).ToString()
            ));

        var dtos = faker.Generate(count);

        foreach (var dto in dtos)
        {
            var result = await officeService.CreateOfficeAsync(dto, ct);
            if (result.IsSuccess) createdIds.Add(result.Value);
        }

        return createdIds;
    }

    private async Task AssignOfficesToProfessorsAsync(List<Guid> officeIds, List<Guid> professorIds, Action<string> log,
        CancellationToken ct)
    {
        log("Assigning offices...");
        var availableOffices = officeIds.OrderBy(x => Guid.NewGuid()).ToList();

        var assigned = 0;
        for (var i = 0; i < professorIds.Count; i++)
        {
            if (i >= availableOffices.Count) break;
            await officeService.AssignProfessorToOfficeAsync(availableOffices[i], professorIds[i], ct);
            assigned++;
        }

        log($"Assigned {assigned} professors to offices.");
    }

    private async Task<List<Guid>> SeedProfessorsAsync(int count, Action<string> log, CancellationToken ct)
    {
        log($"Seeding {count} Professors...");
        var createdIds = new List<Guid>();

        var faker = new Faker<CreateProfessorDto>("pl")
            .CustomInstantiator(f => new CreateProfessorDto(
                f.Name.FirstName(),
                f.Name.LastName(),
                f.Address.StreetName(),
                f.Address.City(),
                f.Address.ZipCode(),
                f.PickRandom<AcademicTitle>()
            ));

        var dtos = faker.Generate(count);

        foreach (var dto in dtos)
        {
            var result = await professorService.CreateProfessorAsync(dto, ct);
            if (result.IsSuccess) createdIds.Add(result.Value);
        }

        return createdIds;
    }

    private async Task<List<Guid>> SeedCoursesAsync(int count, Action<string> log, CancellationToken ct)
    {
        var departmentIds = await context.Departments.Select(d => d.Id).ToListAsync(ct);
        if (departmentIds.Count == 0) return [];

        var professorIds = await context.Professors.Select(p => p.Id).ToListAsync(ct);
        if (professorIds.Count == 0) return [];

        log($"Seeding {count} courses...");
        var createdIds = new List<Guid>();

        var faker = new Faker<CreateCourseDto>("pl")
            .CustomInstantiator(f => new CreateCourseDto(
                f.Company.CatchPhrase(),
                f.Random.Replace("???-###"),
                f.Random.Int(2, 8),
                f.PickRandom(departmentIds),
                f.PickRandom(professorIds)
            ));

        var dtos = faker.Generate(count);
        foreach (var dto in dtos)
        {
            var result = await courseService.CreateCourseAsync(dto, ct);
            if (result.IsSuccess) createdIds.Add(result.Value);
        }

        return createdIds;
    }

    private async Task<List<Guid>> SeedStudentsAsync(int count, Action<string> log, CancellationToken ct)
    {
        log($"Seeding {count} students...");
        var createdIds = new List<Guid>();

        var faker = new Faker<CreateStudentDto>("pl")
            .CustomInstantiator(f => new CreateStudentDto(
                f.Name.FirstName(),
                f.Name.LastName(),
                f.Address.StreetName(),
                f.Address.City(),
                f.Address.ZipCode(),
                f.Random.Int(1, 3)
            ));

        var dtos = faker.Generate(count);
        foreach (var dto in dtos)
        {
            var result = await studentService.CreateStudentAsync(dto, ct);
            if (result.IsSuccess) createdIds.Add(result.Value);
        }

        return createdIds;
    }

    private async Task<List<Guid>> SeedMasterStudentsAsync(int count, List<Guid> professorIds, Action<string> log,
        CancellationToken ct)
    {
        if (professorIds == null || professorIds.Count == 0)
            professorIds = await context.Professors.Select(p => p.Id).ToListAsync(ct);

        if (professorIds.Count == 0) return [];

        log($"Seeding {count} Master Students...");
        var createdIds = new List<Guid>();

        var faker = new Faker<CreateMasterStudentDto>("pl")
            .CustomInstantiator(f => new CreateMasterStudentDto(
                f.Name.FirstName(),
                f.Name.LastName(),
                f.Random.Int(4, 5),
                f.Address.StreetName(),
                f.Address.City(),
                f.Address.ZipCode(),
                f.Lorem.Sentence(4),
                f.PickRandom(professorIds)
            ));

        var dtos = faker.Generate(count);
        foreach (var dto in dtos)
        {
            var result = await masterStudentService.CreateMasterStudentAsync(dto, ct);
            if (result.IsSuccess) createdIds.Add(result.Value);
        }

        return createdIds;
    }

    private async Task SeedEnrollmentsAsync(List<Guid> studentIds, List<Guid> courseIds, Action<string> log,
        CancellationToken ct)
    {
        log("Seeding Enrollments & Grades...");
        var random = new Random();
        var enrollmentsCount = 0;

        foreach (var studentId in studentIds)
        {
            var coursesToEnroll = courseIds.OrderBy(x => random.Next()).Take(random.Next(2, 5)).ToList();

            foreach (var courseId in coursesToEnroll)
            {
                var enrollDto = new EnrollStudentDto(studentId, courseId, "2025/Lato");
                var enrollResult = await enrollmentService.EnrollStudentAsync(enrollDto, ct);

                if (enrollResult.IsSuccess)
                {
                    enrollmentsCount++;
                    if (random.NextDouble() > 0.2)
                    {
                        var grades = new[] { 2.0, 3.0, 3.5, 4.0, 4.5, 5.0 };
                        var grade = grades[random.Next(grades.Length)];
                        await enrollmentService.GradeStudentAsync(new UpdateGradeDto(enrollResult.Value, grade), ct);
                    }
                }
            }
        }

        log($"Created {enrollmentsCount} enrollments.");
    }
}
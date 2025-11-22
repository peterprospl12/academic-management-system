using AMS.Application.Common.Interfaces;
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
    IOfficeService officeService)
{
    private const int ProfessorCount = 5;
    private const int StudentCount = 20;
    private const int MasterStudentCount = 5;
    private const int CourseCount = 8;

    public async Task SeedAsync(CancellationToken cancellationToken)
    {
        if (await context.Students.AnyAsync(cancellationToken))
        {
            Console.WriteLine("Database is already seeded. Skipping");
            return;
        }

        Console.WriteLine("Seeding data has been started");

        await SeedDepartmentsAsync(cancellationToken);

        var officeIds = await SeedOfficesAsync(cancellationToken);

        var professorIds = await SeedProfessorsAsync(cancellationToken);

        await AssignOfficesToProfessorsAsync(officeIds, professorIds, cancellationToken);

        var courseIds = await SeedCoursesAsync(cancellationToken);

        var studentIds = await SeedStudentsAsync(cancellationToken);

        var masterIds = await SeedMasterStudentsAsync(professorIds, cancellationToken);

        var allStudentIds = studentIds.Concat(masterIds).ToList();
        await SeedEnrollmentsAsync(allStudentIds, courseIds, cancellationToken);

        Console.WriteLine("\n\n--- Seeding finished ---");
    }

    private async Task SeedDepartmentsAsync(CancellationToken ct)
    {
        Console.WriteLine("Seeding departments...");
        var departments = new[]
            { "Wydział Informatyki", "Wydział Matematyki", "Wydział Fizyki", "Wydział Zarządzania" };

        foreach (var name in departments)
            await departmentService.CreateDepartmentAsync(new CreateDepartmentDto(name), ct);
    }

    private async Task<List<Guid>> SeedOfficesAsync(CancellationToken ct)
    {
        Console.Write("Seeding offices: ");
        var createdIds = new List<Guid>();
        var faker = new Faker<CreateOfficeDto>("pl")
            .CustomInstantiator(f => new CreateOfficeDto(
                "Budynek " + f.PickRandom("A", "B", "C", "D"),
                f.Random.Number(100, 499).ToString()
            ));

        var dtos = faker.Generate(ProfessorCount + 2);

        foreach (var dto in dtos)
        {
            var result = await officeService.CreateOfficeAsync(dto, ct);
            if (result.IsSuccess)
            {
                createdIds.Add(result.Value);
                Console.Write("O");
            }
        }

        Console.WriteLine();
        return createdIds;
    }

    private async Task AssignOfficesToProfessorsAsync(List<Guid> officeIds, List<Guid> professorIds,
        CancellationToken ct)
    {
        Console.Write("Przypisywanie biur: ");
        var availableOffices = officeIds.OrderBy(x => Guid.NewGuid()).ToList();

        for (var i = 0; i < professorIds.Count; i++)
        {
            if (i >= availableOffices.Count) break;

            var profId = professorIds[i];
            var officeId = availableOffices[i];

            await officeService.AssignProfessorToOfficeAsync(officeId, profId, ct);
            Console.Write(".");
        }

        Console.WriteLine();
    }

    private async Task<List<Guid>> SeedProfessorsAsync(CancellationToken ct)
    {
        Console.Write("Seeding Professors: ");
        var createdIds = new List<Guid>();

        var faker = new Faker<CreateProfessorDto>("pl")
            .CustomInstantiator(f => new CreateProfessorDto(
                f.Name.FirstName(),
                f.Name.LastName(),
                f.Address.StreetName() + " " + f.Address.BuildingNumber(),
                f.Address.City(),
                f.Address.ZipCode(),
                f.PickRandom<AcademicTitle>()
            ));

        var dtos = faker.Generate(ProfessorCount);

        foreach (var dto in dtos)
        {
            var result = await professorService.CreateProfessorAsync(dto, ct);
            if (result.IsSuccess)
            {
                createdIds.Add(result.Value);
                Console.Write("P");
            }
        }

        Console.WriteLine();
        return createdIds;
    }

    private async Task<List<Guid>> SeedCoursesAsync(CancellationToken ct)
    {
        var departmentIds = await context.Departments
            .Select(d => d.Id)
            .ToListAsync(ct);

        if (departmentIds.Count == 0)
        {
            Console.WriteLine("\n[ERROR] No departments in database! Cannot create courses.");
            return [];
        }

        var professorIds = await context.Professors.Select(p => p.Id).ToListAsync(ct);

        if (professorIds.Count == 0) Console.WriteLine("\n[ERROR] No professors in database! Cannot create courses.");

        Console.Write("Seeding courses: ");
        var createdIds = new List<Guid>();

        var faker = new Faker<CreateCourseDto>("pl")
            .CustomInstantiator(f => new CreateCourseDto(
                f.Company.CatchPhrase(),
                f.Random.Replace("???-###"),
                f.Random.Int(2, 8),
                f.PickRandom(departmentIds),
                f.PickRandom(professorIds)
            ));

        var dtos = faker.Generate(CourseCount);

        foreach (var dto in dtos)
        {
            var result = await courseService.CreateCourseAsync(dto, ct);
            if (result.IsSuccess)
            {
                createdIds.Add(result.Value);
                Console.Write("C");
            }
        }

        Console.WriteLine();

        Console.Write("Seeding Prerequisites: ");
        var random = new Random();
        foreach (var courseId in createdIds)
            if (random.NextDouble() > 0.7)
            {
                var prereqId = createdIds[random.Next(createdIds.Count)];
                if (prereqId != courseId)
                {
                    await courseService.AddPrerequisiteAsync(courseId, prereqId, ct);
                    Console.Write("*");
                }
            }

        Console.WriteLine();
        return createdIds;
    }

    private async Task<List<Guid>> SeedStudentsAsync(CancellationToken ct)
    {
        Console.Write("Seeding students: ");
        var createdIds = new List<Guid>();

        var faker = new Faker<CreateStudentDto>("pl")
            .CustomInstantiator(f => new CreateStudentDto(
                f.Name.FirstName(),
                f.Name.LastName(),
                f.Address.StreetName() + " " + f.Address.BuildingNumber(),
                f.Address.City(),
                f.Address.ZipCode(),
                f.Random.Int(1, 3)
            ));

        var dtos = faker.Generate(StudentCount);

        foreach (var dto in dtos)
        {
            var result = await studentService.CreateStudentAsync(dto, ct);
            if (result.IsSuccess)
            {
                createdIds.Add(result.Value);
                Console.Write("S");
            }
        }

        Console.WriteLine();
        return createdIds;
    }

    private async Task<List<Guid>> SeedMasterStudentsAsync(List<Guid> professorIds, CancellationToken ct)
    {
        Console.Write("Seeding Master Students: ");
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

        var dtos = faker.Generate(MasterStudentCount);

        foreach (var dto in dtos)
        {
            var result = await masterStudentService.CreateMasterStudentAsync(dto, ct);
            if (result.IsSuccess)
            {
                createdIds.Add(result.Value);
                Console.Write("M");
            }
        }

        Console.WriteLine();
        return createdIds;
    }

    private async Task SeedEnrollmentsAsync(List<Guid> studentIds, List<Guid> courseIds, CancellationToken ct)
    {
        Console.Write("Seeding Enrollments: ");
        var random = new Random();

        foreach (var studentId in studentIds)
        {
            var coursesToEnroll = courseIds.OrderBy(x => random.Next()).Take(random.Next(2, 5)).ToList();

            foreach (var courseId in coursesToEnroll)
            {
                var enrollDto = new EnrollStudentDto(studentId, courseId, "2025/Lato");
                var enrollResult = await enrollmentService.EnrollStudentAsync(enrollDto, ct);

                if (enrollResult.IsSuccess)
                {
                    Console.Write(".");
                    if (random.NextDouble() > 0.2)
                    {
                        var grades = new[] { 2.0, 3.0, 3.5, 4.0, 4.5, 5.0 };
                        var grade = grades[random.Next(grades.Length)];

                        await enrollmentService.GradeStudentAsync(
                            new UpdateGradeDto(enrollResult.Value, grade), ct);
                    }
                }
            }
        }

        Console.WriteLine();
    }
}
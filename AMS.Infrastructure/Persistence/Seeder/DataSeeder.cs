using AMS.Application.Common.Interfaces;
using AMS.Domain.Entities;
using AMS.Domain.Enums;
using AMS.Domain.ValueObjects;
using Bogus;
using Microsoft.EntityFrameworkCore;

namespace AMS.Infrastructure.Persistence.Seeder;

public class DataSeeder(IApplicationDbContext context)
{
    private const int StudentCount = 50;
    private const int MasterStudentCount = 10;
    private const int ProfessorCount = 5;
    private const int CourseCount = 10;

    public async Task SeedAsync(CancellationToken cancellationToken)
    {
        if (await context.Students.AnyAsync(cancellationToken)) return;


        var studentCounter = new SequenceCounter { Prefix = "S", CurrentValue = 1000 };
        var professorCounter = new SequenceCounter { Prefix = "P", CurrentValue = 100 };

        await context.SequenceCounters.AddRangeAsync(new[] { studentCounter, professorCounter }, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        var faker = new Faker("pl");

        var departments = new List<Department>
        {
            new() { Name = "Wydział Informatyki i Zarządzania" },
            new() { Name = "Wydział Elektroniki" },
            new() { Name = "Wydział Matematyki" }
        };
        await context.Departments.AddRangeAsync(departments, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        var professors = new List<Professor>();
        var offices = new List<Office>();

        var professorFaker = new Faker<Professor>("pl")
            .RuleFor(p => p.FirstName, f => f.Name.FirstName())
            .RuleFor(p => p.LastName, f => f.Name.LastName())
            .RuleFor(p => p.AcademicTitle, f => f.PickRandom<AcademicTitle>())
            .RuleFor(p => p.UniversityIndex, (_, _) =>
            {
                professorCounter.CurrentValue++;
                return $"{professorCounter.Prefix}{professorCounter.CurrentValue}";
            })
            .RuleFor(p => p.Address, f => new Address(
                f.Address.StreetName() + " " + f.Address.BuildingNumber(),
                f.Address.City(),
                f.Address.ZipCode()
            ));

        var officeFaker = new Faker<Office>("pl")
            .RuleFor(o => o.Building, f => "Budynek " + f.PickRandom("A", "B", "C", "D"))
            .RuleFor(o => o.RoomNumber, f => f.Random.Number(100, 400).ToString());

        for (var i = 0; i < ProfessorCount; i++)
        {
            var professor = professorFaker.Generate();
            var office = officeFaker.Generate();

            office.Professor = professor;

            professors.Add(professor);
            offices.Add(office);
        }

        await context.Professors.AddRangeAsync(professors, cancellationToken);
        await context.Offices.AddRangeAsync(offices, cancellationToken);

        var courseFaker = new Faker<Course>("pl")
            .RuleFor(c => c.Name, f => f.Company.CatchPhrase())
            .RuleFor(c => c.CourseCode, f => f.Random.Replace("###-???"))
            .RuleFor(c => c.Ects, f => f.Random.Int(2, 8));

        var courses = courseFaker.Generate(CourseCount);
        await context.Courses.AddRangeAsync(courses, cancellationToken);

        await context.SaveChangesAsync(cancellationToken);

        var random = new Random();
        foreach (var course in courses)
        {
            if (random.NextDouble() > 0.7)
            {
                var potentialPrerequisite = courses[random.Next(courses.Count)];
                if (potentialPrerequisite.Id != course.Id)
                {
                    course.Prerequisites.Add(potentialPrerequisite);
                }
            }
        }

        var studentFaker = new Faker<Student>("pl")
            .RuleFor(s => s.FirstName, f => f.Name.FirstName())
            .RuleFor(s => s.LastName, f => f.Name.LastName())
            .RuleFor(s => s.YearOfStudy, f => f.Random.Int(1, 3))
            .RuleFor(s => s.UniversityIndex, (_, _) =>
            {
                studentCounter.CurrentValue++;
                return $"{studentCounter.Prefix}{studentCounter.CurrentValue}";
            })
            .RuleFor(s => s.Address, f => new Address(
                f.Address.StreetName(),
                f.Address.City(),
                f.Address.ZipCode()
            ));

        var students = studentFaker.Generate(StudentCount);

        var masterStudentFaker = new Faker<MasterStudent>("pl")
            .RuleFor(s => s.FirstName, f => f.Name.FirstName())
            .RuleFor(s => s.LastName, f => f.Name.LastName())
            .RuleFor(s => s.YearOfStudy, f => f.Random.Int(4, 5))
            .RuleFor(s => s.UniversityIndex, (_, _) =>
            {
                studentCounter.CurrentValue++;
                return $"{studentCounter.Prefix}{studentCounter.CurrentValue}";
            })
            .RuleFor(s => s.Address, f => new Address(f.Address.StreetName(), f.Address.City(), f.Address.ZipCode()))
            .RuleFor(m => m.ThesisTopic, f => f.Lorem.Sentence(5))
            .RuleFor(m => m.Promoter, f => f.PickRandom(professors));

        var masterStudents = masterStudentFaker.Generate(MasterStudentCount);

        var allStudents = new List<Student>();
        allStudents.AddRange(students);
        allStudents.AddRange(masterStudents);

        await context.Students.AddRangeAsync(allStudents, cancellationToken);

        var enrollments = new List<Enrollment>();
        foreach (var student in allStudents)
        {
            var coursesToEnroll = faker.PickRandom(courses, faker.Random.Int(3, 5)).ToList();

            foreach (var course in coursesToEnroll)
            {
                enrollments.Add(new Enrollment
                {
                    Student = student,
                    Course = course,
                    Semester = "2025/Summer",
                    Grade = random.NextDouble() > 0.2 ? GetRandomGrade(random) : null
                });
            }
        }

        await context.Enrollments.AddRangeAsync(enrollments, cancellationToken);

        await context.SaveChangesAsync(cancellationToken);
    }

    private static double GetRandomGrade(Random random)
    {
        var grades = new[] { 2.0, 3.0, 3.5, 4.0, 4.5, 5.0 };
        return grades[random.Next(grades.Length)];
    }
}
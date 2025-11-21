using AMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace AMS.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Student> Students { get; }
    DbSet<MasterStudent> MasterStudents { get; }
    DbSet<Professor> Professors { get; }
    DbSet<Course> Courses { get; }
    DbSet<Enrollment> Enrollments { get; }
    DbSet<Department> Departments { get; }
    DbSet<Office> Offices { get; }
    DbSet<SequenceCounter> SequenceCounters { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);

    DatabaseFacade Database { get; }
}
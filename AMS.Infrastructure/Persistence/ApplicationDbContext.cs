using System.Reflection;
using AMS.Application.Common.Interfaces;
using AMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AMS.Infrastructure.Persistence;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : DbContext(options), IApplicationDbContext
{
    public DbSet<Student> Students { get; set; } = null!;
    public DbSet<MasterStudent> MasterStudents { get; set; } = null!;
    public DbSet<Professor> Professors { get; set; } = null!;
    public DbSet<Course> Courses { get; set; } = null!;
    public DbSet<Enrollment> Enrollments { get; set; } = null!;
    public DbSet<Department> Departments { get; set; } = null!;
    public DbSet<Office> Offices { get; set; } = null!;
    public DbSet<SequenceCounter> SequenceCounters { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
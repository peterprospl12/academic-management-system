using AMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AMS.Infrastructure.Persistence.Configurations;

public class CourseConfiguration : IEntityTypeConfiguration<Course>
{
    public void Configure(EntityTypeBuilder<Course> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Name).HasMaxLength(200).IsRequired();
        builder.Property(c => c.CourseCode).HasMaxLength(20).IsRequired();

        builder.Property(c => c.Ects).HasPrecision(4, 1).IsRequired();

        builder.HasOne(c => c.Department)
            .WithMany()
            .HasForeignKey(c => c.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.Lecturer)
            .WithMany()
            .HasForeignKey(c => c.LecturerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(c => c.Prerequisites)
            .WithMany(c => c.IsPrerequisiteFor)
            .UsingEntity<Dictionary<string, object>>(
                "CoursePrerequisites",
                right => right.HasOne<Course>()
                    .WithMany()
                    .HasForeignKey("PrerequisiteId")
                    .OnDelete(DeleteBehavior
                        .Restrict),
                left => left.HasOne<Course>()
                    .WithMany()
                    .HasForeignKey("CourseId")
                    .OnDelete(DeleteBehavior.Cascade),
                join => { join.ToTable("CoursePrerequisites"); }
            );
    }
}
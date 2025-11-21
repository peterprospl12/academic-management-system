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

        builder.HasMany(c => c.Prerequisites)       // Kurs ma wymagania
            .WithMany(c => c.IsPrerequisiteFor)     // Kurs jest wymaganiem dla innych
            .UsingEntity<Dictionary<string, object>>(
                "CoursePrerequisites", // Nazwa tabeli w SQL

                // Konfiguracja "Prawej" strony (Target - czyli Prerequisite)
                right => right.HasOne<Course>()
                    .WithMany()
                    .HasForeignKey("PrerequisiteId")
                    .OnDelete(DeleteBehavior.Restrict), // Ważne: Usunięcie prerekwizytu nie powinno kaskadowo usuwać kursu głównego!

                // Konfiguracja "Lewej" strony (Source - czyli Course)
                left => left.HasOne<Course>()
                    .WithMany()
                    .HasForeignKey("CourseId")
                    .OnDelete(DeleteBehavior.Cascade), // Usunięcie kursu usuwa wpisy w tabeli łączącej

                // Konfiguracja samej tabeli łączącej (opcjonalne klucze)
                join =>
                {
                    join.ToTable("CoursePrerequisites");
                    // Opcjonalnie: klucz główny złożony (zazwyczaj EF robi to sam, ale można wymusić)
                    // join.HasKey("CourseId", "PrerequisiteId");
                }
            );
    }
}

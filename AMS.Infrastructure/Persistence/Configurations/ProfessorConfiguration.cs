using AMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AMS.Infrastructure.Persistence.Configurations;

public class ProfessorConfiguration : IEntityTypeConfiguration<Professor>
{
    public void Configure(EntityTypeBuilder<Professor> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.FirstName)
            .IsRequired()
            .HasMaxLength(100);
        builder.Property(p => p.LastName)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(p => p.UniversityIndex).IsUnique();
        builder.Property(p => p.UniversityIndex).IsRequired().HasMaxLength(20);

        builder.OwnsOne(p => p.Address, addressBuilder =>
        {
            addressBuilder.WithOwner();

            addressBuilder.Property(a => a.Street)
                .HasMaxLength(200)
                .HasColumnName("Address_Street");

            addressBuilder.Property(a => a.City)
                .HasMaxLength(100)
                .HasColumnName("Address_City");

            addressBuilder.Property(a => a.PostalCode)
                .HasMaxLength(20)
                .HasColumnName("Address_PostalCode");
        });
    }
}
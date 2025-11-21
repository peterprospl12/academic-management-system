using AMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AMS.Infrastructure.Persistence.Configurations;

public class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.FirstName).IsRequired().HasMaxLength(100);
        builder.Property(s => s.LastName).IsRequired().HasMaxLength(100);

        builder.HasIndex(s => s.UniversityIndex).IsUnique();
        builder.Property(s => s.UniversityIndex).IsRequired().HasMaxLength(20);

        builder.OwnsOne(s => s.Address, addressBuilder =>
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

        builder.HasDiscriminator<string>("StudentType")
            .HasValue<Student>("Standard")
            .HasValue<MasterStudent>("Master");
    }
}
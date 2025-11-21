using AMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AMS.Infrastructure.Persistence.Configurations;

public class OfficeConfiguration : IEntityTypeConfiguration<Office>
{
    public void Configure(EntityTypeBuilder<Office> builder)
    {
        builder.HasKey(o => o.Id);

        builder.Property(o => o.RoomNumber).IsRequired();
        builder.Property(o => o.Building).IsRequired().HasMaxLength(100);

        builder.HasOne(o => o.Professor)
            .WithOne(p => p.Office)
            .HasForeignKey<Office>(o => o.ProfessorId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
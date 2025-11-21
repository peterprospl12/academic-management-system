using AMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AMS.Infrastructure.Persistence.Configurations;

public class MasterStudentConfiguration : IEntityTypeConfiguration<MasterStudent>
{
    public void Configure(EntityTypeBuilder<MasterStudent> builder)
    {
        builder.Property(m => m.ThesisTopic)
            .HasMaxLength(500)
            .IsRequired();

        builder.HasOne(m => m.Promoter)
            .WithMany()
            .HasForeignKey(m => m.PromoterId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
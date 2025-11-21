using AMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AMS.Infrastructure.Persistence.Configurations;

public class SequenceCounterConfiguration : IEntityTypeConfiguration<SequenceCounter>
{
    public void Configure(EntityTypeBuilder<SequenceCounter> builder)
    {
        builder.HasKey(sc => sc.Prefix);

        builder.Property(sc => sc.Prefix)
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(sc => sc.CurrentValue)
            .IsConcurrencyToken();
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RealEstateApp.Core.Domain.Entities;

namespace RealEstateApp.Infrastructure.Persistence.Configurations
{
    public sealed class PropertyValuationConfiguration : IEntityTypeConfiguration<PropertyValuation>
    {
        public void Configure(EntityTypeBuilder<PropertyValuation> builder)
        {
            builder.ToTable("PropertyValuations");
            builder.HasKey(pv => pv.Id);

            builder.Property(pv => pv.EstimatedPricePerSqm)
                .HasColumnType("decimal(18,2)");

            builder.Property(pv => pv.EstimatedTotalPrice)
                .HasColumnType("decimal(18,2)");

            builder.Property(pv => pv.MinPricePerSqm)
                .HasColumnType("decimal(18,2)");

            builder.Property(pv => pv.MaxPricePerSqm)
                .HasColumnType("decimal(18,2)");

            builder.Property(pv => pv.StandardDeviation)
                .HasColumnType("decimal(18,2)");

            builder.Property(pv => pv.SearchRadiusKm)
                .HasColumnType("decimal(8,2)");

            builder.Property(pv => pv.Currency)
                .IsRequired()
                .HasMaxLength(3);

            builder.Property(pv => pv.Notes)
                .HasMaxLength(1000);

            builder.HasOne(pv => pv.Property)
                .WithMany()
                .HasForeignKey(pv => pv.PropertyId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(pv => pv.PropertyId);
        }
    }
}

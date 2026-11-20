using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RealEstateApp.Core.Domain.Entities;

namespace RealEstateApp.Infrastructure.Persistence.Configurations
{
    public sealed class PropertyPriceHistoryConfiguration : IEntityTypeConfiguration<PropertyPriceHistory>
    {
        public void Configure(EntityTypeBuilder<PropertyPriceHistory> builder)
        {
            builder.ToTable("PropertyPriceHistories");
            builder.HasKey(ph => ph.Id);

            builder.Property(ph => ph.OldPrice)
                .HasColumnType("decimal(18,2)");

            builder.Property(ph => ph.NewPrice)
                .HasColumnType("decimal(18,2)");

            builder.Property(ph => ph.PercentageChange)
                .HasColumnType("decimal(8,2)");

            builder.Property(ph => ph.Currency)
                .IsRequired()
                .HasMaxLength(10);

            builder.Property(ph => ph.ChangeReason)
                .HasMaxLength(250);

            builder.Property(ph => ph.ChangedByUserId)
                .HasMaxLength(450);

            builder.HasOne(ph => ph.Property)
                .WithMany(p => p.PriceHistories)
                .HasForeignKey(ph => ph.PropertyId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(ph => ph.PropertyId);
            builder.HasIndex(ph => ph.ChangeDate);
        }
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RealEstateApp.Core.Domain.Entities;

namespace RealEstateApp.Infrastructure.Persistence.Configurations
{
    public sealed class SavedSearchConfiguration : IEntityTypeConfiguration<SavedSearch>
    {
        public void Configure(EntityTypeBuilder<SavedSearch> builder)
        {
            builder.ToTable("SavedSearches");
            builder.HasKey(s => s.Id);

            builder.Property(s => s.UserId)
                .IsRequired()
                .HasMaxLength(450);

            builder.Property(s => s.Name)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(s => s.Sector)
                .HasMaxLength(100);

            builder.Property(s => s.MinPrice)
                .HasColumnType("decimal(18,2)");

            builder.Property(s => s.MaxPrice)
                .HasColumnType("decimal(18,2)");

            builder.Property(s => s.MinSizeInMeters)
                .HasColumnType("decimal(18,2)");

            builder.Property(s => s.MaxSizeInMeters)
                .HasColumnType("decimal(18,2)");

            builder.HasIndex(s => s.UserId);
        }
    }
}

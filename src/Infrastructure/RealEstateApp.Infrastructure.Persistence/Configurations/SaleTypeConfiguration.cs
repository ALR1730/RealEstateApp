using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RealEstateApp.Core.Domain.Entities;

namespace RealEstateApp.Infrastructure.Persistence.Configurations
{
    public sealed class SaleTypeConfiguration : IEntityTypeConfiguration<SaleType>
    {
        public void Configure(EntityTypeBuilder<SaleType> builder)
        {
            builder.ToTable("SaleTypes");
            builder.HasKey(st => st.Id);

            builder.Property(st => st.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(st => st.Description)
                .HasMaxLength(500);
        }
    }
}

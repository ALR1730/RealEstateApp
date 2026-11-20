using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RealEstateApp.Core.Domain.Constants;
using RealEstateApp.Core.Domain.Entities;

namespace RealEstateApp.Infrastructure.Persistence.Configurations
{
    public sealed class PropertyConfiguration : IEntityTypeConfiguration<Property>
    {
        public void Configure(EntityTypeBuilder<Property> builder)
        {
            builder.ToTable("Properties");
            builder.HasKey(p => p.Id);

            builder.Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(p => p.Code)
                .IsRequired()
                .HasMaxLength(15);

            builder.HasIndex(p => p.Code)
                .IsUnique();

            builder.Property(p => p.Price)
                .HasColumnType("decimal(18,2)");

            builder.Property(p => p.Currency)
                .IsRequired()
                .HasMaxLength(3)
                .HasDefaultValue(CurrencyConstants.DOP);

            builder.Property(p => p.PriceInDOP)
                .HasColumnType("decimal(18,2)");

            builder.Property(p => p.SizeInMeters)
                .HasColumnType("decimal(18,2)");

            builder.Property(p => p.MontoSeparacion)
                .HasColumnType("decimal(18,2)");

            builder.Property(p => p.Description)
                .HasMaxLength(2000);

            builder.Property(p => p.MatterportModelId)
                .HasMaxLength(50);

            builder.Property(p => p.Status)
                .HasMaxLength(20)
                .HasDefaultValue(PropertyStatus.Available);

            builder.Property(p => p.AgentId)
                .IsRequired()
                .HasMaxLength(450);

            builder.Property(p => p.Sector)
                .HasMaxLength(100);

            builder.Property(p => p.FullAddress)
                .HasMaxLength(300);

            builder.Property(p => p.IsFeatured)
                .HasDefaultValue(false);

            builder.Property(p => p.RowVersion)
                .IsConcurrencyToken()
                .HasDefaultValue(1)
                .ValueGeneratedNever();

            builder.HasOne(p => p.PropertyType)
                .WithMany(pt => pt.Properties)
                .HasForeignKey(p => p.PropertyTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(p => p.SaleType)
                .WithMany(st => st.Properties)
                .HasForeignKey(p => p.SaleTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(p => p.Province)
                .WithMany(prov => prov.Properties)
                .HasForeignKey(p => p.ProvinceId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(p => p.Municipality)
                .WithMany(mun => mun.Properties)
                .HasForeignKey(p => p.MunicipalityId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}

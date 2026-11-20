using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RealEstateApp.Core.Domain.Constants;
using RealEstateApp.Core.Domain.Entities;

namespace RealEstateApp.Infrastructure.Persistence.Configurations
{
    public sealed class CommissionConfiguration : IEntityTypeConfiguration<Commission>
    {
        public void Configure(EntityTypeBuilder<Commission> builder)
        {
            builder.ToTable("Commissions");
            builder.HasKey(c => c.Id);

            builder.Property(c => c.AgentId)
                .IsRequired()
                .HasMaxLength(450);

            builder.Property(c => c.SalePrice)
                .HasColumnType("decimal(18,2)");

            builder.Property(c => c.Rate)
                .HasColumnType("decimal(5,2)");

            builder.Property(c => c.Amount)
                .HasColumnType("decimal(18,2)");

            builder.Property(c => c.Status)
                .IsRequired()
                .HasMaxLength(20)
                .HasDefaultValue(CommissionStatus.Pending);

            builder.HasOne(c => c.Property)
                .WithMany()
                .HasForeignKey(c => c.PropertyId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(c => c.Offer)
                .WithMany()
                .HasForeignKey(c => c.OfferId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(c => c.AgentId);
            builder.HasIndex(c => c.Status);
        }
    }
}

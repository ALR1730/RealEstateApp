using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RealEstateApp.Core.Domain.Entities;

namespace RealEstateApp.Infrastructure.Persistence.Configurations
{
    public sealed class OfferConfiguration : IEntityTypeConfiguration<Offer>
    {
        public void Configure(EntityTypeBuilder<Offer> builder)
        {
            builder.ToTable("Offers");
            builder.HasKey(o => o.Id);

            builder.Property(o => o.MontoOfertado)
                .HasColumnType("decimal(18,2)");

            builder.Property(o => o.CounterOfferAmount)
                .HasColumnType("decimal(18,2)");

            builder.Property(o => o.CounterOfferMessage)
                .HasMaxLength(1000);

            builder.Property(o => o.Notes)
                .HasMaxLength(1000);

            builder.Property(o => o.ClienteId)
                .IsRequired()
                .HasMaxLength(450);

            builder.Property(o => o.RowVersion)
                .IsConcurrencyToken()
                .HasDefaultValue(1)
                .ValueGeneratedNever();

            builder.HasOne(o => o.Property)
                .WithMany(p => p.Offers)
                .HasForeignKey(o => o.PropertyId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

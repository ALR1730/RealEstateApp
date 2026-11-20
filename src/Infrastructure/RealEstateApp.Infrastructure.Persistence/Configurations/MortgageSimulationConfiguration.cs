using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RealEstateApp.Core.Domain.Entities;

namespace RealEstateApp.Infrastructure.Persistence.Configurations
{
    public sealed class MortgageSimulationConfiguration : IEntityTypeConfiguration<MortgageSimulation>
    {
        public void Configure(EntityTypeBuilder<MortgageSimulation> builder)
        {
            builder.ToTable("MortgageSimulations");
            builder.HasKey(ms => ms.Id);

            builder.Property(ms => ms.MontoInicialAportado)
                .HasColumnType("decimal(18,2)");

            builder.Property(ms => ms.TasaInteresAnual)
                .HasColumnType("decimal(5,2)");

            builder.Property(ms => ms.ClienteId)
                .IsRequired()
                .HasMaxLength(450);

            builder.HasOne(ms => ms.Property)
                .WithMany(p => p.MortgageSimulations)
                .HasForeignKey(ms => ms.PropertyId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

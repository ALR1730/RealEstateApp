using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RealEstateApp.Core.Domain.Entities;

namespace RealEstateApp.Infrastructure.Persistence.Configurations
{
    public sealed class PropertyAppointmentConfiguration : IEntityTypeConfiguration<PropertyAppointment>
    {
        public void Configure(EntityTypeBuilder<PropertyAppointment> builder)
        {
            builder.ToTable("PropertyAppointments");
            builder.HasKey(pa => pa.Id);

            builder.Property(pa => pa.ClienteId)
                .IsRequired()
                .HasMaxLength(450);

            builder.Property(pa => pa.AgentId)
                .IsRequired()
                .HasMaxLength(450);

            builder.Property(pa => pa.Notes)
                .HasMaxLength(1000);

            builder.Property(pa => pa.AgentNotes)
                .HasMaxLength(1000);

            builder.HasOne(pa => pa.Property)
                .WithMany(p => p.Appointments)
                .HasForeignKey(pa => pa.PropertyId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
